using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

using AutomaticAds.Managers;

namespace AutomaticAds.Services;

public class ScreenTextService
{
    private readonly Dictionary<CCSPlayerController, CPointWorldText> _playerTexts = new();
    private readonly TimerManager _timerManager;
    private readonly float _displayTime;

    public float PositionX { get; set; } = -1.8f;
    public float PositionY { get; set; } = 1f;

    public ScreenTextService(TimerManager timerManager, float displayTime = 5.0f)
    {
        _timerManager = timerManager;
        _displayTime = displayTime;
    }

    public void ShowTextOnScreen(CCSPlayerController player, string text)
    {
        if (!player.IsValid || !player.PawnIsAlive)
            return;

        HideTextFromScreen(player);

        var viewModel = EnsureCustomViewModel(player);
        if (viewModel == null)
        {
            Console.WriteLine($"[AutomaticAds] Error: Could not create ViewModel for {player.PlayerName}");
            return;
        }

        var vectorData = CalculateTextPosition(player);
        if (!vectorData.HasValue)
        {
            Console.WriteLine($"[AutomaticAds] Error: Could not calculate position for {player.PlayerName}");
            return;
        }

        var textEntity = CreateWorldTextEntity(
            text: text,
            fontSize: 25,
            color: Color.Yellow,
            fontName: "Tahoma Bold",
            position: vectorData.Value.Position,
            angle: vectorData.Value.Angle,
            viewModel: viewModel,
            depthOffset: 0.0f
        );

        if (textEntity != null)
        {
            _playerTexts[player] = textEntity;
            _timerManager.AddTimer(_displayTime, () => HideTextFromScreen(player));
        }
    }

    public void HideTextFromScreen(CCSPlayerController player)
    {
        if (_playerTexts.TryGetValue(player, out var textEntity))
        {
            if (textEntity?.IsValid == true)
                textEntity.Remove();
            _playerTexts.Remove(player);
        }
    }

    public void ClearAllPlayerTexts()
    {
        foreach (var textEntity in _playerTexts.Values)
        {
            if (textEntity?.IsValid == true)
                textEntity.Remove();
        }
        _playerTexts.Clear();
    }

    public void OnPlayerDisconnect(CCSPlayerController player)
    {
        HideTextFromScreen(player);
    }

    private CCSGOViewModel? EnsureCustomViewModel(CCSPlayerController player)
    {
        var pawn = GetPlayerPawn(player);
        if (pawn?.ViewModelServices == null) return null;

        try
        {
            int offset = Schema.GetSchemaOffset("CCSPlayer_ViewModelServices", "m_hViewModel");
            IntPtr viewModelHandleAddress = pawn.ViewModelServices.Handle + offset + 4;

            var handle = new CHandle<CCSGOViewModel>(viewModelHandleAddress);

            if (!handle.IsValid)
            {
                var viewModel = Utilities.CreateEntityByName<CCSGOViewModel>("predicted_viewmodel");
                if (viewModel != null)
                {
                    viewModel.DispatchSpawn();
                    handle.Raw = viewModel.EntityHandle.Raw;
                    Utilities.SetStateChanged(pawn, "CCSPlayerPawnBase", "m_pViewModelServices");
                }
            }

            return handle.Value;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error creating ViewModel: {ex.Message}");
            return null;
        }
    }

    private CCSPlayerPawn? GetPlayerPawn(CCSPlayerController player)
    {
        if (player.Pawn.Value is not CBasePlayerPawn pawn) return null;

        if (pawn.LifeState == (byte)LifeState_t.LIFE_DEAD)
        {
            if (pawn.ObserverServices?.ObserverTarget.Value?.As<CBasePlayerPawn>() is CBasePlayerPawn observer)
            {
                pawn = observer;
            }
            else
            {
                return null;
            }
        }

        return pawn.As<CCSPlayerPawn>();
    }

    public readonly record struct VectorData(Vector Position, QAngle Angle);

    private VectorData? CalculateTextPosition(CCSPlayerController player)
    {
        var playerPawn = GetPlayerPawn(player);
        if (playerPawn == null) return null;

        try
        {
            QAngle eyeAngles = playerPawn.EyeAngles;

            Vector forward = new(), right = new(), up = new();
            NativeAPI.AngleVectors(eyeAngles.Handle, forward.Handle, right.Handle, up.Handle);

            var adjustedPositions = ApplyFOVAdjustment(player, PositionX, PositionY);

            Vector textOffset = forward * 7.0f + right * adjustedPositions.X + up * adjustedPositions.Y;
            Vector finalPosition = playerPawn.AbsOrigin! + textOffset + new Vector(0, 0, playerPawn.ViewOffset.Z);

            QAngle textAngle = new()
            {
                Y = eyeAngles.Y + 270.0f,
                Z = 90.0f - eyeAngles.X,
                X = 0.0f
            };

            return new VectorData(finalPosition, textAngle);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error calculating text position: {ex.Message}");
            return null;
        }
    }

    private (float X, float Y) ApplyFOVAdjustment(CCSPlayerController player, float x, float y)
    {
        float fov = player.DesiredFOV == 0 ? 90 : player.DesiredFOV;

        if (fov == 90)
            return (x, y);

        float scaleFactor = (float)Math.Tan((fov / 2) * Math.PI / 180) / (float)Math.Tan(45 * Math.PI / 180);

        return (x * scaleFactor, y * scaleFactor);
    }

    private CPointWorldText? CreateWorldTextEntity(
        string text,
        int fontSize,
        Color color,
        string fontName,
        Vector position,
        QAngle angle,
        CCSGOViewModel viewModel,
        float depthOffset = 0.1f)
    {
        try
        {
            var entity = Utilities.CreateEntityByName<CPointWorldText>("point_worldtext");
            if (entity == null || !entity.IsValid) return null;

            entity.MessageText = text;
            entity.Enabled = true;
            entity.FontSize = fontSize;
            entity.FontName = fontName;
            entity.Fullbright = true;
            entity.Color = color;

            entity.WorldUnitsPerPx = 0.0085f;
            entity.BackgroundWorldToUV = 0.01f;
            entity.JustifyHorizontal = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT;
            entity.JustifyVertical = PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_CENTER;
            entity.ReorientMode = PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE;
            entity.RenderMode = RenderMode_t.kRenderNormal;

            entity.DrawBackground = true;
            entity.BackgroundBorderHeight = 0.1f;
            entity.BackgroundBorderWidth = 0.1f;

            entity.DepthOffset = depthOffset;

            entity.DispatchSpawn();
            entity.Teleport(position, angle, null);

            entity.AcceptInput("SetParent", viewModel, null, "!activator");

            return entity;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error creating text entity: {ex.Message}");
            return null;
        }
    }
}