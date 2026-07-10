using System.Collections;
using System.Collections.Generic;
using Kogetsu.Library.DesignPatternCore;
using TMPro;
using UnityEngine;

public class QuestDisplay : MonoBehaviour
{
    [SerializeField] private List<string> _quests = new();
    [SerializeField] private TMP_Text _text;
    [SerializeField] private float _charDelay = 0.05f;

    private int _currentIndex;
    private Coroutine _typeCoroutine;

    private void Start()
    {
        if (_quests.Count > 0)
            RunTypewriter(_quests[0]);
    }

    private void OnEnable()
    {
        if (EventBus.Instance)
            EventBus.Instance.Subscribe<NextQuestEvent>(OnNextQuest);
    }

    private void OnDisable()
    {
        if (EventBus.Instance)
            EventBus.Instance.Unsubscribe<NextQuestEvent>(OnNextQuest);
    }

    private void OnNextQuest(NextQuestEvent _)
    {
        if (_currentIndex + 1 >= _quests.Count) return;

        _currentIndex++;

        if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);
        _typeCoroutine = StartCoroutine(TransitionToNext(_quests[_currentIndex]));
    }

    private void RunTypewriter(string text)
    {
        if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);
        _typeCoroutine = StartCoroutine(TypeText(text));
    }

    private IEnumerator TransitionToNext(string newText)
    {
        // ลบทีละตัวอักษรจนหมด
        string current = _text.text;
        while (current.Length > 0)
        {
            current = current[..^1];
            _text.text = current;
            yield return new WaitForSeconds(_charDelay);
        }

        // เขียนทีละตัวอักษร
        yield return TypeText(newText);
    }

    private IEnumerator TypeText(string text)
    {
        _text.text = "";
        foreach (char c in text)
        {
            _text.text += c;
            yield return new WaitForSeconds(_charDelay);
        }
        _typeCoroutine = null;
    }
}
