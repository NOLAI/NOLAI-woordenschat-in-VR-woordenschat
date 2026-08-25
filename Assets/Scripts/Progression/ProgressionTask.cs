/*
 * ProgressionTask.cs - Defines task types and structure for game progression
 *
 * This script defines the core progression system:
 * - Enumerates different task types (pickup, move, press, etc.)
 * - Defines movement types (translate or rotate)
 * - Contains all task configuration data as a ScriptableObject
 * - References grab scripts for specific task interactions
 */

using UnityEngine;
using UnityEngine.Events; //Used for Events
using System.Collections.Generic; //Used for Lists
using UnityEngine.InputSystem;


public enum TaskType { PickUpObject, MoveToPosition, PressButton, MoveObjectToLocation, JustAudio, PointAtEachObject, GrabAndPull}
public enum MovementType { Translate, Rotate}

[System.Serializable]
public class ProgressionTask
{
    [Tooltip("Wat voor een type taak de speler moet voltooien")]
    public TaskType taskType;
    [Tooltip("Welke uitleg er moet verschijnen op het tekstveld")]
    public string description;
    [Tooltip("Object waar de gebruiker een interactie mee aan moet gaan")]
    public List<GameObject> targetObjects;
    [Tooltip("Object waar de targets naartoe moeten bewegen. gebruikt voor de MoveObjectToLocation taak")]
    public GameObject FinishTargetObject;


    [Tooltip("Movement type voor GrabAndPull taak, hoe moet het object bewegen")]
    public MovementType movementType;
    [Tooltip("Movement translator voor omhoogtrekken, welk object moet zijn limiet halen om voltooid te worden?")]
    public GrabMovementTranslate grabMovementScript; // voor anker
    [Tooltip("Movement translator voor draaien, welk object moet zijn limiet halen om voltooid te worden?")]
    public GrabRotatePlank grabRotateScript; // voor loopplank

    [Tooltip("Buttons required to complete the task (used only for PressButton tasks)")]
    public List<InputActionReference> buttonActions;

    [Tooltip("Audio om te spelen op het begin van de taak")]
    public AudioClip StartAudioClip;
    [Tooltip("Audio om te spelen wanneer verkeert object opgepakt wordt")]
    public AudioClip WrongObjectAudioClip;
    [Tooltip("Audio om te spelen op het einde van de taak")]
    public AudioClip completionAudioClip;

    [Tooltip("Verwacht je dat de gebruiker iets uitspreekt nadat de audio is afgespeelt?")]
    public bool ExpectTalking = false;
    [Tooltip("Wil je dat objecten met dezeflde tags ook als finish objecten worden gezien?")]
    public bool SameTagsAlsoFinish = false;

    [Tooltip("Events om uit te voeren op het begin van de taak")]
    public UnityEvent TaskStarted;
    [Tooltip("Events om uit te voeren op het einde van de taak")]
    public UnityEvent TaskFinished;
}