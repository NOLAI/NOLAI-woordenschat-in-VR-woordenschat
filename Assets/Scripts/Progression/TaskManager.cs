/*
 * TaskManager.cs - Manages and executes progression tasks
 *
 * This script is the central controller for game progression:
 * - Loads and executes tasks in sequence
 * - Displays task descriptions to the player
 * - Plays audio cues for task start/completion
 * - Handles microphone recording for voice analysis
 * - Manages help functions (outlines, etc.)
 * - Requires ProgressionTask.cs to define what needs to be completed in a task
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR; // Added for XR input devices
using UnityEngine.XR.Interaction.Toolkit;
//using static UnityEditor.Rendering.FilterWindow;

public class TaskManager : MonoBehaviour
{
    public TextMeshProUGUI taskDescriptionText;
    public AudioSource audioSource;

    [Tooltip("Audio clip to play when the user is expected to talk")]
    public AudioClip PlingSound;
    MicrophoneRecorder microphoneRecorder;
    [Tooltip("Whether to record audio for voice analysis")]
    public bool shouldRecord = false;
    [Tooltip("Recording duration in seconds")]
    public float MicrophoneTime = 5;

    [Tooltip("Wait time before starting first task")]
    public float StartTimeOffset = 5;

    [Tooltip("Event triggered at the start of the phase")]
    public UnityEvent PhaseStart;
    [Tooltip("Wait time before enabling help functions")]
    public int HelpWaitTime = 10;
    public ProgressionTask[] tasks;

    [Tooltip("Sub-phase number")]
    public int ElementId = 1;
    [Tooltip("Phase number")]
    public int Fase = 1;
    public UpdateCurrentElement updateCurrentElement;


    private int currentTaskIndex = 0;

    private Queue<AudioClip> audioQueue = new Queue<AudioClip>();
    private bool isPlayingAudioQueue = false;

    private XRGrabInteractable currentGrabInteractable;
    private bool IsPhaseRunning = false;

    private Outline[] Outlines;
    private List<GameObject> CompletedPickups = new List<GameObject>();

    private List<TaskManager> taskManagers = new List<TaskManager>();
    private List<XRRayInteractor> rayInteractors = new List<XRRayInteractor>();
    public AudioClip LastPlayedAudioClip = null;
    private Outline outline;

    public void StartPhase()
    {
        // Disable all outlines
        List<Outline> outlinesList = new List<Outline>();
        foreach (var task in tasks)
        {
            foreach (var targetObject in task.targetObjects)
            {
                if (targetObject.TryGetComponent(out Outline outline))
                {
                    outlinesList.Add(outline);
                }
            }
        }

        Outlines = outlinesList.ToArray();
        foreach (Outline outline in Outlines)
        {
            outline.enabled = false;
        }

        // Kill all other task managers in the scene
        foreach (var AltTaskManager in taskManagers)
        {
            if (AltTaskManager != this)
            {
                AltTaskManager.KillPhase();
            }
        }

        // Send current element and phase to backend
        if (updateCurrentElement != null)
        {
            updateCurrentElement.UpdateCurrentElementToBackend(ElementId, Fase);
            Debug.Log("New phase, updateCurrentElement called with " + ElementId + " " + Fase);
        }

        // Start the task
        StartCoroutine(WaitAndStartTask());

    }

    public void KillPhase()
    {
        audioSource.Stop();
        audioQueue.Clear();
        StopCoroutine(WaitAndStartTask());

        if (currentTaskIndex < 0 || currentTaskIndex >= tasks.Length)
        {
            Debug.LogWarning("KillPhase called with an invalid currentTaskIndex.");
            IsPhaseRunning = false;
            return;
        }

        try
        {
            var currentTask = tasks[currentTaskIndex];
            foreach (var targetObject in currentTask.targetObjects)
            {
                if (targetObject == null) continue;
                if (targetObject.TryGetComponent<Outline>(out Outline outline))
                {
                    outline = targetObject.GetComponent<Outline>();
                    if (outline != null)
                    {
                        outline.enabled = false;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"KillPhase error catch: {e}");
        }
        IsPhaseRunning = false;
    }

    void Start()
    {
        // Find all other task managers in the scene
        TaskManager[] taskManagers = FindObjectsOfType<TaskManager>();
        foreach (var taskManager in taskManagers)
        {
            if (taskManager != this)
            {
                this.taskManagers.Add(taskManager);
            }
        }
    }

    IEnumerator WaitAndStartTask()
    {
        yield return new WaitForSeconds(StartTimeOffset);
        currentTaskIndex = 0;
        PhaseStart?.Invoke();
        IsPhaseRunning = true;
        CheckMoveToPositionTasks();
        UpdateTask();
    }
    

    void FindRayInteractors()
    {
        //Find all ray interactor in the scene
        XRRayInteractor[] rayInteractors = FindObjectsOfType<XRRayInteractor>();
        foreach (var rayInteractor in rayInteractors)
        {
            if (rayInteractor != null && rayInteractor.gameObject != this.gameObject)
            {
                this.rayInteractors.Add(rayInteractor);
                Debug.Log("Found ray interactor: " + rayInteractor.gameObject.name);
            }
        }
    }

    void DisableRayInteractors()
    {
        Debug.Log("Disabling Ray interactors. Amount found: " + rayInteractors.Count);
        foreach (var rayInteractor in rayInteractors)
        {
            rayInteractor.enabled = false;
        }
    }
    void EnableRayInteractors()
    {
        Debug.Log("Enabling Ray interactors");
        foreach (var rayInteractor in rayInteractors)
        {
            rayInteractor.enabled = true;
        }
    }

    void CheckMoveToPositionTasks()
    {
        foreach (var task in tasks)
        {
            if (task.taskType == TaskType.MoveToPosition && task.targetObjects != null)
            {
                foreach (var targetObject in task.targetObjects)
                {
                    if (targetObject != null)
                    {
                        Collider collider = targetObject.GetComponent<Collider>();

                        if (collider == null)
                        {
                            Debug.LogWarning($"Task '{task.description}' targetObject '{targetObject.name}' has no Collider. Your task cannot finish because of this.");
                        }
                        else if (!collider.isTrigger)
                        {
                            Debug.LogWarning($"Task '{task.description}' targetObject '{targetObject.name}' has a Collider, but 'isTrigger' is not enabled. Your task will never finish!");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Task '{task.description}' has a null targetObject in its list.");
                    }
                }
            }
        }
    }

    void UpdateTask()
    {
        EnableRayInteractors();

        if(currentTaskIndex >= tasks.Length)
        {
            Debug.Log("All tasks completed. Phase is finished.");
            IsPhaseRunning = false;
            return;
        }
        
        if (currentTaskIndex < tasks.Length)
        {
            var currentTask = tasks[currentTaskIndex];
            if (currentTaskIndex > tasks.Length)
            {
                Debug.LogWarning("Task index is higher than the amount of tasks in the list. Phase is finished.");
                IsPhaseRunning = false;
                return;
            }

            Debug.Log("Task number " + currentTaskIndex + " started of type " + currentTask.taskType);
            currentTask.TaskStarted?.Invoke();
            if (currentTask.taskType == TaskType.PressButton) //If the task needs a button press, light up Interaction button with this function
            {
                ColorSwitcher.TriggerStartSwitching();
            }
            else
            {
                ColorSwitcher.TriggerStopSwitching();
            }

            if (currentTask.StartAudioClip != null)
            {
                EnqueueAudio(currentTask.StartAudioClip);

                if (currentTask.ExpectTalking)
                {
                    Debug.Log("Expecting user to talk after audio is finished.");
                    StartCoroutine(WaitForAudioToEndCoroutine());
                }
            }

            if (taskDescriptionText != null)
            {
                taskDescriptionText.text = currentTask.description;
            }

            if (currentTask.taskType == TaskType.PickUpObject || currentTask.taskType == TaskType.PointAtEachObject || currentTask.taskType == TaskType.MoveObjectToLocation)
            {
                foreach (var targetObject in currentTask.targetObjects)
                {
                    StartCoroutine(EnableOutlineAfterTalk(targetObject, HelpWaitTime)); //If the user has not found the object(s) after 10 seconds, enable the outlines
                }
            }

            if (currentTask.taskType == TaskType.GrabAndPull)
            {
    

                if (currentTask.movementType == MovementType.Translate && currentTask.grabMovementScript != null)
                {
                    // Remove existing listener
                    currentTask.grabMovementScript.OnLimitReached.RemoveListener(HandleLimitReached);
                    // Add new listener
                    currentTask.grabMovementScript.OnLimitReached.AddListener(HandleLimitReached);
                    var targetObject = currentTask.grabMovementScript.gameObject;
                    StartCoroutine(EnableOutlineAfterTalk(targetObject, HelpWaitTime));
                }
                else if (currentTask.movementType == MovementType.Rotate && currentTask.grabRotateScript != null)
                {
                    // Verwijder eerst eventuele bestaande listener
                    currentTask.grabRotateScript.OnLimitReached.RemoveListener(HandleLimitReached);
                    // Voeg dan de nieuwe listener toe
                    currentTask.grabRotateScript.OnLimitReached.AddListener(HandleLimitReached);
                    var targetObject = currentTask.grabRotateScript.gameObject;
                    StartCoroutine(EnableOutlineAfterTalk(targetObject, HelpWaitTime));
                }

                

            }

            SetupGrabListener(currentTask);
        }
    }


    void SetupGrabListener(ProgressionTask currentTask)
    {
        // Remove the listener from the previous interactable, if any
        if (currentGrabInteractable != null)
        {
            currentGrabInteractable.selectEntered.RemoveListener(OnGrab);
            currentGrabInteractable = null;
        }

        // Find all XRGrabInteractable objects in the scene
        XRGrabInteractable[] allInteractables = FindObjectsOfType<XRGrabInteractable>();

        foreach (var interactable in allInteractables)
        {
            // Add the OnGrab listener to all interactables
            interactable.selectEntered.AddListener(OnGrab);
        }
    }

    void Update()
    { //Runs every frame, so don't do any intense processing here!
        if (!IsPhaseRunning) return;

        if (currentTaskIndex < tasks.Length && !audioSource.isPlaying && tasks[currentTaskIndex].taskType == TaskType.PressButton)
        {
            CheckControllerGripPress(XRNode.RightHand);
            CheckControllerGripPress(XRNode.LeftHand);
        }
        if (currentTaskIndex < tasks.Length && !audioSource.isPlaying && tasks[currentTaskIndex].taskType == TaskType.JustAudio)
        {
            if (audioSource.isPlaying)
            {
                return;
            }
            else
            {
                if (!tasks[currentTaskIndex].ExpectTalking)
                {
                    CompleteTask();
                }
            }
        }
        if (currentTaskIndex < tasks.Length && tasks[currentTaskIndex].taskType == TaskType.MoveObjectToLocation)
        {
            var currentTask = tasks[currentTaskIndex];
            if (currentTask.FinishTargetObject != null)
            {
                var targetCollider = currentTask.FinishTargetObject.GetComponent<Collider>();
                if (targetCollider != null && targetCollider.isTrigger)
                {
                    for (int i = currentTask.targetObjects.Count - 1; i >= 0; i--)
                    {
                        var targetObject = currentTask.targetObjects[i];
                        if (targetObject != null && targetCollider.bounds.Contains(targetObject.transform.position))
                        {
                            Debug.Log($"Object '{targetObject.name}' moved to the correct location.");
                            CompleteTask();
                            Destroy(targetObject); // Disable target object after completing the task to avoid issues
                            currentTask.targetObjects.RemoveAt(i); // Remove the destroyed object from the list
                            break;
                        }
                    }
                }
            }
        }

        if(rayInteractors.Count == 0)
        {
            FindRayInteractors();
        }

        if (currentTaskIndex < tasks.Length && tasks[currentTaskIndex].taskType == TaskType.GrabAndPull)
        {
            var currentTask = tasks[currentTaskIndex];
            if (currentTask.movementType == MovementType.Translate && currentTask.grabMovementScript != null)
            {
                currentTask.grabMovementScript.OnLimitReached.AddListener(HandleLimitReached);
            }
            else if (currentTask.movementType == MovementType.Rotate && currentTask.grabRotateScript != null)
            {
                currentTask.grabRotateScript.OnLimitReached.AddListener(HandleLimitReached);
            }
        }

    }

    private void HandleLimitReached(float limit)
    {


        foreach (Outline outline in Outlines)
        {
            outline.enabled = false;
        }
        CompleteTask();

    }

    void CheckControllerGripPress(XRNode hand) //Just monitor the grip buttons and finish task if they are pressed during call.
    {
        var inputDevice = InputDevices.GetDeviceAtXRNode(hand);
        bool isPressed;
        if (inputDevice.TryGetFeatureValue(CommonUsages.gripButton, out isPressed) && isPressed)
        {
            Debug.Log($"Grip button pressed on {hand} controller.");
            CompleteTask();
        }
    }

    public void EnqueueAudio(AudioClip clip) //Manage clips to only play one after another
    {
        if (clip != null)
        {
            audioQueue.Enqueue(clip);

            if (!isPlayingAudioQueue)
            {
                StartCoroutine(PlayAudioQueue());
            }
        }
    }

    IEnumerator PlayAudioQueue() //Actually go through the audio queue
    {
        isPlayingAudioQueue = true;

        while (audioQueue.Count > 0)
        {
            AudioClip clip = audioQueue.Dequeue();
            if (clip != LastPlayedAudioClip) {// Make sure not to repeat the same clip twice.
                Debug.Log("Playing audio clip: " + clip.name);
            }
            else
            {
                Debug.Log("Skipping duplicate audio clip: " + clip.name);
                continue; // Skip playing the same clip again
            }
            // Play the audio clip
            audioSource.PlayOneShot(clip);
            taskManagers.ForEach(taskManager => taskManager.LastPlayedAudioClip = clip); // Update the LastPlayedAudioClip for all task managers
            yield return new WaitForSeconds(clip.length);
        }

        isPlayingAudioQueue = false;
    }

    private bool canCompleteTask = true;

    void CompleteTask()
    {
        if (canCompleteTask)
        {
            DisableRayInteractors();
            canCompleteTask = false;
            CompletedPickups.Clear();
            StartCoroutine(WaitForAudioAndCompleteTask());
            StartCoroutine(ResetCompleteTaskCooldown());
        }
    }

    IEnumerator ResetCompleteTaskCooldown()
    {
        yield return new WaitForSeconds(3f);
        canCompleteTask = true;
    }

    IEnumerator WaitForAudioAndCompleteTask()
    {
        while (audioSource.isPlaying)
        {
            yield return new WaitForSeconds(1f);
        }

        if (currentTaskIndex < tasks.Length)
        {
            var currentTask = tasks[currentTaskIndex];

            if (currentTask.completionAudioClip != null)
            {
                Debug.Log("Playing completion audio clip.");
                EnqueueAudio(currentTask.completionAudioClip);

                // Wait until the audio queue is empty
                while (audioQueue.Count > 0 || audioSource.isPlaying)
                {
                    yield return new WaitForSeconds(1f);
                }
            }

            currentTask.TaskFinished?.Invoke();

            currentTaskIndex++;
            yield return new WaitForSeconds(1f); // wait to finish task in case player has to teleport to new location
            UpdateTask(); // Progress to next task
        }
    }



    void OnGrab(SelectEnterEventArgs args)
    {
        Debug.Log("OnGrab event triggered.");
        XRGrabInteractable grabbedObject = args.interactableObject as XRGrabInteractable;

        // Play audio if script has defined audio clip
        var playAudioOnGrab = grabbedObject.GetComponent<PlayAudioOnGrab>();
        if (playAudioOnGrab != null && playAudioOnGrab.GetSelectedAudioClip() != null && !playAudioOnGrab.HasPlayed)
        {
            //EnqueueAudio(playAudioOnGrab.GetSelectedAudioClip());
            audioSource.Stop();
             taskManagers.ForEach(taskManager => taskManager.LastPlayedAudioClip = playAudioOnGrab.GetSelectedAudioClip());
            if (LastPlayedAudioClip != playAudioOnGrab.GetSelectedAudioClip())
            {
                Debug.Log("Playing audio clip: " + playAudioOnGrab.GetSelectedAudioClip().name);
            }
            else
            {
                Debug.Log("Skipping duplicate audio clip: " + playAudioOnGrab.GetSelectedAudioClip().name);
                return; // Skip playing the same clip again
            }
            playAudioOnGrab.HasPlayed = true;
            DisableRayInteractors();
            audioSource.PlayOneShot(playAudioOnGrab.GetSelectedAudioClip());
            //Once the clip has finished, enable the ray interactors again
            StartCoroutine(EnableRayInteractorsAfterAudio(playAudioOnGrab.GetSelectedAudioClip().length));
        }



        // Disable outline on grabbed object
        var outline = grabbedObject.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
        }

        CompletedPickups.Add(grabbedObject.gameObject);

        //PickUpObject task
        if (currentTaskIndex < tasks.Length && tasks[currentTaskIndex].taskType == TaskType.PickUpObject)
        {
            var currentTask = tasks[currentTaskIndex];

            if (grabbedObject != null)
            {
                if (currentTask.targetObjects.Any(obj => obj == grabbedObject.gameObject))
                {
                    Debug.Log($"Correct object '{grabbedObject.gameObject.name}' grabbed.");
                    CompleteTask();
                }
                else
                {
                    Debug.Log($"Incorrect object '{grabbedObject.gameObject.name}' grabbed.");
                    if (currentTask.WrongObjectAudioClip != null)
                    {
                        EnqueueAudio(currentTask.WrongObjectAudioClip);
                    }
                }
            }
        }

        //PointAtEachObject task
        if (currentTaskIndex < tasks.Length && tasks[currentTaskIndex].taskType == TaskType.PointAtEachObject)
        {
            var currentTask = tasks[currentTaskIndex];


            if (grabbedObject != null)
            {
                if (currentTask.SameTagsAlsoFinish)
                {
                    Debug.Log("PointAtEachObject task");

                    // Check if any target object has the same tag as the grabbed object
                    bool isCorrectObject = currentTask.targetObjects.Any(obj => obj.CompareTag(grabbedObject.gameObject.tag));

                    if (isCorrectObject)
                    {
                        Debug.Log($"Correct object '{grabbedObject.gameObject.name}' grabbed.");

                        // Remove any one object of the same tag from the target list
                        GameObject objectToRemove = currentTask.targetObjects.FirstOrDefault(obj => obj.CompareTag(grabbedObject.gameObject.tag));
                        if (objectToRemove != null)
                        {
                            currentTask.targetObjects.Remove(objectToRemove);
                        }

                        Debug.Log($"Remaining objects: {currentTask.targetObjects.Count}");

                        if (currentTask.targetObjects.Count == 0)
                        {
                            Debug.Log("PointAtEachObject task finished!");
                            CompleteTask();
                        }
                    }
                    else
                    {
                        Debug.Log($"Incorrect object '{grabbedObject.gameObject.name}' grabbed.");
                        if (currentTask.WrongObjectAudioClip != null)
                        {
                            EnqueueAudio(currentTask.WrongObjectAudioClip);
                        }
                    }
                }
                else if (currentTask.targetObjects.Contains(grabbedObject.gameObject))
                {
                    Debug.Log($"Correct object '{grabbedObject.gameObject.name}' from list grabbed. Remaining objects: {currentTask.targetObjects.Count - 1}");


                    currentTask.targetObjects.Remove(grabbedObject.gameObject);
                    if (currentTask.targetObjects.Count == 0)
                    {
                        Debug.Log("PointAtEachObject task finished!");
                        CompleteTask();
                    }

                }
                else
                {
                    Debug.Log($"Incorrect object '{grabbedObject.gameObject.name}' grabbed.");
                    if (currentTask.WrongObjectAudioClip != null)
                    {
                        EnqueueAudio(currentTask.WrongObjectAudioClip);
                    }
                }
            }
        }
    }

    // void OnTriggerEnter(Collider other)
    // {
    //     if (currentTaskIndex < tasks.Length && tasks[currentTaskIndex].taskType == TaskType.MoveObjectToLocation)
    //     {
    //         var currentTask = tasks[currentTaskIndex];
    //         if (currentTask.targetObjects.Contains(other.gameObject))
    //         {
    //             Debug.Log($"Object '{other.gameObject.name}' entered the trigger area.");
    //             CompleteTask();
    //             return; //Exit loop to avoid triggering it again and again
    //         }
    //     }
    // }

    IEnumerator EnableRayInteractorsAfterAudio(float delay)
    {
        yield return new WaitForSeconds(delay);
        EnableRayInteractors();
    }

    IEnumerator WaitForAudioToEndCoroutine()
    {
        if (shouldRecord)
        {
            GameObject dashboardController = GameObject.Find("Dashboard connector");
            if (dashboardController != null)
            {
                microphoneRecorder = dashboardController.GetComponent<MicrophoneRecorder>();
                if (microphoneRecorder != null)
                {
                    Debug.Log("MicrophoneRecorder found and connected.");
                    microphoneRecorder.SetElementId(ElementId);
                }
                else
                {
                    Debug.LogWarning("MicrophoneRecorder not found on DashboardController!");
                }
            }
            else
            {
                Debug.LogWarning("DashboardController not found!");
            }
        }

        Debug.Log("Waiting for audio to end before enabling talking icon.");
        yield return new WaitUntil(() => !audioSource.isPlaying);
        Debug.Log("Audio has ended. Enabling talking icon and playing the ping.");
        
        // Start the progress bar
        FindObjectOfType<StatusEffectsManager>().StartProgressBar(MicrophoneTime);


        // Play the pling sound
        if (PlingSound != null)
        {
            audioSource.PlayOneShot(PlingSound);
        }

       

        // Add microphone recorder if needed for validation
        if (microphoneRecorder != null)
        {
            // Start the microphone recording
            Debug.Log("Answer is important. Starting microphone recording...");
            microphoneRecorder.StartRecording();
            yield return new WaitForSeconds(MicrophoneTime);
            microphoneRecorder.StopRecording();

            //Start animation indicating 'thinking'
            Debug.Log("Microphone recording stopped. Start thinking animation.");
            FindObjectOfType<ThinkingAnimationManager>()?.StartThinkingAnimation();
        }
        else
        {
            Debug.Log("No microphone recorder found");
        }
    }

    IEnumerator EnableOutlineAfterTalk(GameObject targetObject, float delay)
    {
        while (audioSource.isPlaying)
        {
            yield return new WaitForSeconds(1f);
        }
        Debug.Log("Enabeling helping outline for " + targetObject.name);
        StartCoroutine(EnableOutline(targetObject, delay));
        yield return null;
    }
    IEnumerator EnableOutline(GameObject targetObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (targetObject != null)
        {
            Outline outline = targetObject.GetComponent<Outline>();

            if (CompletedPickups.Contains(targetObject) == false)
            {
                if (outline != null)
                {
                    outline.enabled = true;
                }
                else
                {
                    var NewOutline = targetObject.AddComponent<Outline>();
                    NewOutline.OutlineMode = Outline.Mode.OutlineVisible;
                    NewOutline.OutlineColor = Color.green;
                    NewOutline.OutlineWidth = 5f;
                }
            }
        }
    }
}

