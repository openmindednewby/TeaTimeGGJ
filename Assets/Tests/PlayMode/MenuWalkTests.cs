using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class MenuWalkTests
{
    private const int FramesToSettle = 2;
    private const int MaxControls = 500;

    private static readonly MoveDirection[] Directions =
    {
        MoveDirection.Up, MoveDirection.Down, MoveDirection.Left, MoveDirection.Right,
    };

    private static IEnumerable<int> BuildScenes() =>
        Enumerable.Range(0, SceneManager.sceneCountInBuildSettings);

    [UnityTest]
    public IEnumerator BuildScene_WalkedByNavigateEvents_ReachesEverySelectable(
        [ValueSource(nameof(BuildScenes))] int sceneIndex)
    {
        LogAssert.ignoreFailingMessages = true;
        SceneManager.LoadScene(sceneIndex);
        for (var frame = 0; frame < FramesToSettle; frame++) yield return null;

        var selectables = Object.FindObjectsOfType<Selectable>()
            .Where(s => s.isActiveAndEnabled && s.IsInteractable())
            .ToList();
        TestContext.WriteLine($"scene {sceneIndex}: {selectables.Count} interactable selectables");
        if (selectables.Count == 0) yield break;

        var eventSystem = EventSystem.current;
        Assert.IsNotNull(eventSystem, "a scene with Selectables has no EventSystem");
        Assert.IsNotNull(eventSystem.firstSelectedGameObject, "the EventSystem has no First Selected object");
        AssertKeyboardAndPadBound(eventSystem.GetComponent<BaseInputModule>());

        var reached = WalkByNavigateEvents(eventSystem);

        var unreached = selectables.Where(s => !reached.Contains(s.gameObject)).Select(s => s.name).ToList();
        Assert.IsEmpty(unreached, $"unreached by arrows / pad: {string.Join(", ", unreached)}");
    }

    private static HashSet<GameObject> WalkByNavigateEvents(EventSystem eventSystem)
    {
        var start = eventSystem.firstSelectedGameObject;
        var reached = new HashSet<GameObject> { start };
        var queue = new Queue<GameObject>(new[] { start });
        while (queue.Count > 0 && reached.Count < MaxControls)
        {
            var from = queue.Dequeue();
            foreach (var direction in Directions)
            {
                var to = Navigate(eventSystem, from, direction);
                if (to != null && reached.Add(to)) queue.Enqueue(to);
            }
        }
        return reached;
    }

    private static GameObject Navigate(EventSystem eventSystem, GameObject from, MoveDirection direction)
    {
        eventSystem.SetSelectedGameObject(from);
        var move = new AxisEventData(eventSystem) { moveDir = direction, moveVector = ToVector(direction) };
        ExecuteEvents.Execute(from, move, ExecuteEvents.moveHandler);
        return eventSystem.currentSelectedGameObject;
    }

    private static Vector2 ToVector(MoveDirection direction) => direction switch
    {
        MoveDirection.Up => Vector2.up,
        MoveDirection.Down => Vector2.down,
        MoveDirection.Left => Vector2.left,
        _ => Vector2.right,
    };

    private static void AssertKeyboardAndPadBound(BaseInputModule module)
    {
        Assert.IsNotNull(module, "the EventSystem has no input module");
        if (module is StandaloneInputModule standalone)
        {
            Assert.IsNotEmpty(standalone.horizontalAxis);
            Assert.IsNotEmpty(standalone.verticalAxis);
            Assert.IsNotEmpty(standalone.submitButton);
            Assert.IsNotEmpty(standalone.cancelButton);
            return;
        }
        var ui = module as InputSystemUIInputModule;
        Assert.IsNotNull(ui, $"unsupported input module {module.GetType().Name}");
        AssertBound(ui.move, "Navigate");
        AssertBound(ui.submit, "Submit");
        AssertBound(ui.cancel, "Cancel");
    }

    private static void AssertBound(InputActionReference reference, string name)
    {
        Assert.IsNotNull(reference?.action, $"{name} action is not assigned");
        var paths = reference.action.bindings.Select(b => b.effectivePath ?? string.Empty).ToList();
        var keyboard = paths.Any(p => p.StartsWith("<Keyboard>") || p.StartsWith("*/"));
        var pad = paths.Any(p => p.StartsWith("<Gamepad>") || p.StartsWith("*/"));
        Assert.IsTrue(keyboard && pad, $"{name} is not bound to both keyboard and gamepad");
    }
}
