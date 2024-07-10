using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using StudioCommunication;

namespace CelesteStudio.Util;

public struct ActionLine() {
    public const char Delimiter = ',';
    public const int MaxFrames = 9999;
    public const int MaxFramesDigits = 4;

    public Actions Actions;
    public int Frames;

    public string? FeatherAngle;
    public string? FeatherMagnitude;

    public HashSet<char> CustomBindings = [];
    
    public static ActionLine? Parse(string line, bool ignoreInvalidFloats = true) => TryParseStrict(line, out var actionLine, ignoreInvalidFloats) ? actionLine : null;
    public static bool TryParse(string line, out ActionLine value, bool ignoreInvalidFloats = true) => TryParseStrict(line, out value, ignoreInvalidFloats) || TryParseLoose(line, out value, ignoreInvalidFloats);
    
    /// Parses action-lines, which mostly follow the correct formatting (for example: "  15,R,Z")
    public static bool TryParseStrict(string line, out ActionLine actionLine, bool ignoreInvalidFloats = true) {
        actionLine = default;
        actionLine.CustomBindings = new HashSet<char>();

        string[] tokens = line.Trim().Split(Delimiter, StringSplitOptions.TrimEntries);
        if (tokens.Length == 0) return false;

        if (!int.TryParse(tokens[0], CultureInfo.InvariantCulture, out actionLine.Frames)) return false;

        for (int i = 1; i < tokens.Length; i++) {
            if (string.IsNullOrWhiteSpace(tokens[i])) continue;

            var action = tokens[i][0].ActionForChar();
            actionLine.Actions |= action;

            // Parse dash-only/move-only/custom bindings
            if (action is Actions.DashOnly) {
                for (int j = 1; j < tokens[i].Length; j++) {
                    actionLine.Actions |= tokens[i][j].ActionForChar().ToDashOnlyActions();
                }
                continue;
            }
            if (action is Actions.MoveOnly) {
                for (int j = 1; j < tokens[i].Length; j++) {
                    actionLine.Actions |= tokens[i][j].ActionForChar().ToMoveOnlyActions();
                }
                continue;
            }
            if (action is Actions.PressedKey) {
                actionLine.CustomBindings = tokens[i][1..].Select(char.ToUpper).ToHashSet();
                continue;
            }

            // Parse feather angle/magnitude
            bool validAngle = true;
            if (action == Actions.Feather && i + 1 < tokens.Length && (validAngle = float.TryParse(tokens[i + 1], CultureInfo.InvariantCulture, out float angle))) {
                if (angle > 360.0f)
                    actionLine.FeatherAngle = "360";
                else if (angle < 0.0f)
                    actionLine.FeatherAngle = "0";
                else
                    actionLine.FeatherAngle = tokens[i + 1];
                i++;

                // Allow empty magnitude, so the comma won't get removed
                bool validMagnitude = true;
                if (i + 1 < tokens.Length && (string.IsNullOrWhiteSpace(tokens[i + 1]) || (validMagnitude = float.TryParse(tokens[i + 1], CultureInfo.InvariantCulture, out float _)))) {
                    // Parse again since it might be an empty string
                    if (float.TryParse(tokens[i + 1], CultureInfo.InvariantCulture, out float magnitude)) {
                        if (magnitude > 1.0f)
                            actionLine.FeatherMagnitude = "1";
                        else if (magnitude < 0.0f)
                            actionLine.FeatherMagnitude = "0";
                        else
                            actionLine.FeatherMagnitude = tokens[i + 1];
                    } else {
                        actionLine.FeatherMagnitude = tokens[i + 1];
                    }

                    i++;
                } else if (!validMagnitude && !ignoreInvalidFloats) {
                    return false;
                }
            } else if (!validAngle && i + 2 < tokens.Length && string.IsNullOrEmpty(tokens[i + 1]) && (validAngle = float.TryParse(tokens[i + 2], CultureInfo.InvariantCulture, out angle))) {
                // Empty angle, treat magnitude as angle
                if (angle > 360.0f)
                    actionLine.FeatherAngle = "360";
                else if (angle < 0.0f)
                    actionLine.FeatherAngle = "0";
                else
                    actionLine.FeatherAngle = tokens[i + 1];
                i += 2;
            } else if (!validAngle && !ignoreInvalidFloats) {
                return false;
            }
        }

        return true;
    }
    
    /// Parses action-lines, which mostly are correct (for example: "1gd")
    private enum ParseState { Frame, Action, DashOnly, MoveOnly, PressedKey, FeatherAngle, FeatherMagnitude }
    public static bool TryParseLoose(string line, out ActionLine actionLine, bool ignoreInvalidFloats = true) {
        actionLine = default;
        actionLine.CustomBindings = new HashSet<char>();
        
        ParseState state = ParseState.Frame;
        string currValue = "";
        
        foreach (char c in line) {
            if (char.IsWhiteSpace(c)) {
                continue;
            }
            
            switch (state) {
                case ParseState.Frame:
                {
                    if (c == Delimiter) {
                        continue;
                    }
                    
                    if (char.IsDigit(c)) {
                        currValue += c;
                    } else {
                        if (!int.TryParse(currValue, out actionLine.Frames)) {
                            // Invalid action-line
                            return false;
                        }
                        currValue = "";
                        goto case ParseState.Action;
                    }
                    break;
                }
                
                case ParseState.Action:
                {
                    if (c == Delimiter) {
                        continue;
                    }
                    
                    var action = c.ActionForChar();
                    actionLine.Actions |= action;
                    state = action switch {
                        Actions.DashOnly => ParseState.DashOnly,
                        Actions.MoveOnly => ParseState.MoveOnly,
                        Actions.PressedKey => ParseState.PressedKey,
                        Actions.Feather => ParseState.FeatherAngle,
                        _ => ParseState.Action,
                    };
                    break;
                }
                
                case ParseState.DashOnly:
                {
                    if (c == Delimiter) {
                        state = ParseState.Action;
                        continue;
                    }
                    
                    var action = c.ActionForChar();
                    if (action is not (Actions.Left or Actions.Right or Actions.Up or Actions.Down)) {
                        goto case ParseState.Action;
                    }
                    actionLine.Actions |= action.ToDashOnlyActions();
                    break;
                }
                
                case ParseState.MoveOnly:
                {
                    if (c == Delimiter) {
                        state = ParseState.Action;
                        continue;
                    }
                    
                    var action = c.ActionForChar();
                    if (action is not (Actions.Left or Actions.Right or Actions.Up or Actions.Down)) {
                        goto case ParseState.Action;
                    }
                    actionLine.Actions |= action.ToMoveOnlyActions();
                    break;
                }
                
                case ParseState.PressedKey:
                {
                    if (c == Delimiter) {
                        state = ParseState.Action;
                        continue;
                    }
                    
                    actionLine.CustomBindings.Add(char.ToUpper(c));
                    break;
                }
                
                case ParseState.FeatherAngle:
                {
                    if (c == Delimiter) {
                        state = ParseState.FeatherMagnitude;
                        continue;
                    }
                    
                    if (char.IsDigit(c) || c == '.') {
                        actionLine.FeatherAngle ??= string.Empty;
                        actionLine.FeatherAngle += c;
                    } else {
                        goto case ParseState.Action;
                    }
                    break;
                }
                
                case ParseState.FeatherMagnitude:
                {
                    if (c == Delimiter) {
                        state = ParseState.Action;
                        continue;
                    }
                    
                    if (char.IsDigit(c) || c == '.') {
                        actionLine.FeatherMagnitude ??= string.Empty;
                        actionLine.FeatherMagnitude += c;
                    } else {
                        goto case ParseState.Action;
                    }
                    break;
                }
            }
        }
        
        // Clamp angle / magnitude
        if (actionLine.FeatherAngle is { } angleString) {
            if (float.TryParse(angleString, CultureInfo.InvariantCulture, out float angle)) {
                actionLine.FeatherAngle = Math.Clamp(angle, 0.0f, 360.0f).ToString(CultureInfo.InvariantCulture);
            } else if (!ignoreInvalidFloats) {
                return false;
            }
        }
        if (actionLine.FeatherMagnitude is { } magnitudeString) {
            if (float.TryParse(magnitudeString, CultureInfo.InvariantCulture, out float magnitude)) {
                actionLine.FeatherMagnitude = Math.Clamp(magnitude, 0.0f, 1.0f).ToString(CultureInfo.InvariantCulture);
            } else if (!ignoreInvalidFloats) {
                return false;
            }
        }
        
        return state != ParseState.Frame;
    }

    public override string ToString() {
        var tasActions = Actions;
        var customBindings = CustomBindings.ToList();
        customBindings.Sort();

        string frames = Frames.ToString().PadLeft(MaxFramesDigits);
        string actions = Actions.Sorted().Aggregate("", (s, a) => $"{s}{Delimiter}{a switch {
            Actions.DashOnly => $"{Actions.DashOnly.CharForAction()}{string.Join("", tasActions.GetDashOnly().Select(ActionsUtils.CharForAction))}",
            Actions.MoveOnly => $"{Actions.MoveOnly.CharForAction()}{string.Join("", tasActions.GetMoveOnly().Select(ActionsUtils.CharForAction))}",
            Actions.PressedKey => $"{Actions.PressedKey.CharForAction()}{string.Join("", customBindings)}",
            _ => a.CharForAction().ToString(),
        }}");
        string featherAngle = Actions.HasFlag(Actions.Feather) ? $"{Delimiter}{FeatherAngle ?? ""}" : string.Empty;
        string featherMagnitude = Actions.HasFlag(Actions.Feather) && FeatherMagnitude != null ? $"{Delimiter}{FeatherMagnitude}" : string.Empty;

        return $"{frames}{actions}{featherAngle}{featherMagnitude}";
    }
}