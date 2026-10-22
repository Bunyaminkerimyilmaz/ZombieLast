using System;
using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine.InputSystem;
using Names = LastHaven.Player.InputActionNames;

namespace LastHaven.Player
{
    /// <summary>
    /// GDD §4 klavye/fare ve gamepad bağlamaları, tamamen kodda (.inputactions asset'i yok).
    /// Her <see cref="InputMode"/> için bir harita vardır ve aynı anda sadece biri açıktır.
    /// Birden fazla modda geçerli girdiler (ör. Pause, Ready) her haritada aynı adla tekrar tanımlanır.
    /// </summary>
    public sealed class GameInputActions : IDisposable
    {
        /// <summary>Gameplay'de 1–3 tuşları.</summary>
        public const int WeaponSlotCount = 3;

        /// <summary>BuildMode'da 1–5 tuşları.</summary>
        public const int BuildSlotCount = 5;

        /// <summary>UI'da 1–3 tuşları (seviye kartları).</summary>
        public const int OptionSlotCount = 3;

        private readonly InputActionMap[] _maps;

        public GameInputActions()
        {
            Gameplay = CreateGameplayMap();
            BuildMode = CreateBuildModeMap();
            UI = CreateUIMap();
            _maps = new[] { Gameplay, BuildMode, UI };
            GameplayLocomotion = new LocomotionActions(Gameplay);
            BuildModeLocomotion = new LocomotionActions(BuildMode);
        }

        public InputActionMap Gameplay { get; }
        public InputActionMap BuildMode { get; }
        public InputActionMap UI { get; }
        public IReadOnlyList<InputActionMap> Maps => _maps;

        public LocomotionActions GameplayLocomotion { get; }
        public LocomotionActions BuildModeLocomotion { get; }

        /// <summary>Yuva action'ının adı; <paramref name="slotIndex"/> 0 tabanlı, ad 1 tabanlı ("WeaponSlot1").</summary>
        public static string SlotActionName(string prefix, int slotIndex)
        {
            return prefix + (slotIndex + 1);
        }

        /// <summary>PassThrough eksen değerini yöne çevirir: +1, -1 veya 0 (bırakma/sıfırlanma).</summary>
        public static int ToCycleDirection(float axisValue)
        {
            return axisValue > 0f ? 1 : axisValue < 0f ? -1 : 0;
        }

        public InputActionMap GetMap(InputMode mode)
        {
            return mode switch
            {
                InputMode.Gameplay => Gameplay,
                InputMode.BuildMode => BuildMode,
                InputMode.UI => UI,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null),
            };
        }

        /// <summary>Hareket ve nişan action'ları; UI modunda yoktur (null).</summary>
        public LocomotionActions GetLocomotion(InputMode mode)
        {
            return mode switch
            {
                InputMode.Gameplay => GameplayLocomotion,
                InputMode.BuildMode => BuildModeLocomotion,
                _ => null,
            };
        }

        /// <summary>Sadece <paramref name="mode"/>'un haritasını açar, diğerlerini kapatır.</summary>
        public void Enable(InputMode mode)
        {
            InputActionMap target = GetMap(mode);
            for (int i = 0; i < _maps.Length; i++)
            {
                if (_maps[i] != target)
                {
                    _maps[i].Disable();
                }
            }

            target.Enable();
        }

        public void Disable()
        {
            for (int i = 0; i < _maps.Length; i++)
            {
                _maps[i].Disable();
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < _maps.Length; i++)
            {
                _maps[i].Dispose();
            }
        }

        private static InputActionMap CreateGameplayMap()
        {
            var map = new InputActionMap(nameof(InputMode.Gameplay));
            AddLocomotion(map);
            AddButton(map, Names.Fire, "<Mouse>/leftButton", "<Gamepad>/rightTrigger");
            AddButton(map, Names.Reload, "<Keyboard>/r", "<Gamepad>/buttonWest");
            AddButton(map, Names.Dash, "<Keyboard>/space", "<Gamepad>/buttonSouth");
            AddSlots(map, Names.WeaponSlot, WeaponSlotCount);

            // Tekerlek aşağı = sıradaki (+1), yukarı = önceki (-1); gamepad Y = sıradaki.
            AddCycle(map, Names.WeaponCycle, "<Mouse>/scroll/y", "<Gamepad>/buttonNorth");
            AddButton(map, Names.Interact, "<Keyboard>/e", "<Gamepad>/buttonEast");
            AddButton(map, Names.ToggleBuildMode, "<Keyboard>/b", "<Gamepad>/leftShoulder");
            AddButton(map, Names.Shop, "<Keyboard>/tab", "<Gamepad>/select");
            AddReadyAndPause(map);
            return map;
        }

        private static InputActionMap CreateBuildModeMap()
        {
            var map = new InputActionMap(nameof(InputMode.BuildMode));
            AddLocomotion(map);
            AddButton(map, Names.ToggleBuildMode, "<Keyboard>/b", "<Gamepad>/leftShoulder");
            AddSlots(map, Names.BuildSlot, BuildSlotCount);
            AddCycle(map, Names.BuildCycle, "<Gamepad>/dpad/left", "<Gamepad>/dpad/right");
            AddButton(map, Names.Rotate, "<Keyboard>/q", "<Gamepad>/rightShoulder");
            AddButton(map, Names.Upgrade, "<Keyboard>/u", "<Gamepad>/dpad/up");
            AddButton(map, Names.Sell, "<Keyboard>/x", "<Gamepad>/dpad/down");
            AddButton(map, Names.Confirm, "<Mouse>/leftButton", "<Gamepad>/rightTrigger", "<Gamepad>/buttonSouth");
            AddButton(map, Names.Cancel, "<Mouse>/rightButton", "<Gamepad>/buttonEast");
            AddReadyAndPause(map);
            return map;
        }

        private static InputActionMap CreateUIMap()
        {
            var map = new InputActionMap(nameof(InputMode.UI));
            AddButton(map, Names.Confirm, "<Keyboard>/enter", "<Gamepad>/buttonSouth");
            AddButton(map, Names.Cancel, "<Keyboard>/escape", "<Gamepad>/buttonEast");
            AddButton(map, Names.Shop, "<Keyboard>/tab", "<Gamepad>/select");
            AddSlots(map, Names.OptionSlot, OptionSlotCount);

            // UI'da Ready yok; Start'ı basılı tutmayı beklemeye gerek olmadığı için hemen Pause.
            AddButton(map, Names.Pause, "<Gamepad>/start");
            return map;
        }

        private static void AddLocomotion(InputActionMap map)
        {
            InputAction move = map.AddAction(Names.Move, InputActionType.Value, expectedControlLayout: "Vector2");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            move.AddBinding("<Gamepad>/leftStick");

            // PassThrough: Value olsaydı harita açılırken başlangıç kontrolü fareden tetiklenir ve
            // gamepad kullanan oyuncu yanlışlıkla klavye/fareye geçmiş sayılırdı.
            map.AddAction(Names.AimPointer, InputActionType.PassThrough, "<Mouse>/position", expectedControlLayout: "Vector2");
            map.AddAction(Names.AimStick, InputActionType.Value, "<Gamepad>/rightStick", expectedControlLayout: "Vector2");
        }

        private static void AddReadyAndPause(InputActionMap map)
        {
            AddButton(map, Names.Ready, "<Keyboard>/enter");
            AddButton(map, Names.Pause, "<Keyboard>/escape");

            // Süre: Project Settings → Input System Package → Default Hold Time.
            map.AddAction(Names.GamepadStart, InputActionType.Button, "<Gamepad>/start", interactions: "hold");
        }

        /// <summary>
        /// Sıra değiştirme action'ı (PassThrough eksen). Okuyucu değerin işaretini yön olarak kullanır;
        /// <paramref name="invertedBinding"/> ters çevrilir.
        /// </summary>
        private static void AddCycle(InputActionMap map, string name, string invertedBinding, string binding)
        {
            InputAction action = map.AddAction(name, InputActionType.PassThrough, expectedControlLayout: "Axis");
            action.AddBinding(invertedBinding).WithProcessor("invert");
            action.AddBinding(binding);
        }

        private static void AddSlots(InputActionMap map, string prefix, int count)
        {
            for (int i = 0; i < count; i++)
            {
                AddButton(map, SlotActionName(prefix, i), "<Keyboard>/" + (i + 1));
            }
        }

        private static void AddButton(InputActionMap map, string name, params string[] bindings)
        {
            InputAction action = map.AddAction(name, InputActionType.Button);
            for (int i = 0; i < bindings.Length; i++)
            {
                action.AddBinding(bindings[i]);
            }
        }
    }
}
