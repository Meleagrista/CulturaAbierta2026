using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CubePlayer : MonoBehaviour
{
    [SerializeField] private float rollDuration = 0.18f;

    private Rigidbody m_rigidbody;
    private bool m_isRolling;
    private bool m_isLocked;
    private int m_moveCount;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_rigidbody.isKinematic = true;

        EventManager.StartListening(Event.GAME_WIN_EVENT, OnGameWin);
        EventManager.StartListening(Event.GAME_LOSE_EVENT, OnGameLose);
        EventManager.StartListening(Event.GAME_PAUSE_EVENT, OnPause);
        EventManager.StartListening(Event.GAME_RESUME_EVENT, OnResume);
    }

    private void OnDestroy()
    {
        EventManager.StopListening(Event.GAME_WIN_EVENT, OnGameWin);
        EventManager.StopListening(Event.GAME_LOSE_EVENT, OnGameLose);
        EventManager.StopListening(Event.GAME_PAUSE_EVENT, OnPause);
        EventManager.StopListening(Event.GAME_RESUME_EVENT, OnResume);
    }

    private void Update()
    {
        if (m_isRolling || m_isLocked || Keyboard.current == null)
            return;

        Vector3 direction = ReadDirection(Keyboard.current);
        if (direction != Vector3.zero)
            StartCoroutine(Roll(direction));
    }

    private static Vector3 ReadDirection(Keyboard kb)
    {
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) return Vector3.forward;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) return Vector3.back;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) return Vector3.left;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) return Vector3.right;
        return Vector3.zero;
    }

    private IEnumerator Roll(Vector3 direction)
    {
        m_isRolling = true;

        Vector3 pivot = transform.position + (Vector3.down + direction) * 0.5f;
        Vector3 axis = Vector3.Cross(Vector3.up, direction);
        float rotated = 0f;

        while (rotated < 90f)
        {
            float step = Mathf.Min(90f / rollDuration * Time.deltaTime, 90f - rotated);
            transform.RotateAround(pivot, axis, step);
            rotated += step;
            yield return null;
        }

        transform.SetPositionAndRotation(
            new Vector3(Mathf.Round(transform.position.x), 0.5f, Mathf.Round(transform.position.z)),
            Quaternion.identity);

        m_isRolling = false;
        m_moveCount++;

        EventManager.TriggerEvent(Event.PLAYER_MOVED_EVENT, new Dictionary<string, object>
        {
            { "count", m_moveCount },
            { "position", transform.position }
        });
    }

    private void OnGameWin(Dictionary<string, object> message) => m_isLocked = true;

    private void OnGameLose(Dictionary<string, object> message)
    {
        m_isLocked = true;
        m_rigidbody.isKinematic = false;
    }

    private void OnPause(Dictionary<string, object> message) => m_isLocked = true;
    private void OnResume(Dictionary<string, object> message) => m_isLocked = false;
}
