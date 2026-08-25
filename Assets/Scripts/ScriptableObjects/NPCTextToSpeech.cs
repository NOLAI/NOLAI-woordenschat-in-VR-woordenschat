/*
 * NPCTextToSpeech.cs - ScriptableObject for NPC voice configuration
 *
 * This script defines TTS settings for NPCs:
 * - Model selection (TTS model from OpenAI)
 * - Voice selection (Alloy, Echo, etc.)
 * - Voice settings: speed, pitch, volume
 * - Used as ScriptableObject for asset-based configuration
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TTS/New TTS model", order = 1)]
public class NPCTextToSpeech : ScriptableObject
{
    [Header("General")]
    [SerializeField] private TTSModel model;
    [SerializeField] private TTSVoice voice;

    [Header("Voice Settings")]
    [SerializeField, Range(0.25f, 4.0f)] private float speed = 1f;
    [SerializeField, Range(-3f, 3f)] private float pitch = 1f;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    public TTSModel Model { get => model; }
    public TTSVoice Voice { get => voice; }
    public float Speed { get => speed; }
    public float Pitch { get => pitch; }
    public float Volume { get => volume; }
}