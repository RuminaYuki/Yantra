using Kogetsu.Library.DesignPatternCore;
using UnityEngine;
using UnityEngine.Playables;

public class PlayCutsceneOnInteract : MonoBehaviour
{
    [SerializeField] private PlayableDirector _director;

    private void OnValidate()
    {
        if (_director == null) TryGetComponent(out _director);
    }

    private void OnEnable()
    {
        if (_director != null)
            _director.stopped += OnCutsceneFinished;
    }

    private void OnDisable()
    {
        if (_director != null)
            _director.stopped -= OnCutsceneFinished;
    }

    public void Interact()
    {
        if (_director == null) return;
        _director.Play();
    }

    private void OnCutsceneFinished(PlayableDirector _)
    {
        if (EventBus.Instance)
            EventBus.Instance.Publish(new NextQuestEvent());
    }
}
