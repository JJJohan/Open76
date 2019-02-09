using System.Collections;
using System.Collections.Generic;
using System.Text;
using Assets.Scripts.System;
using Assets.Scripts.System.Fileparsers;
using UnityEngine;

#if !UNITY_EDITOR
using System.IO;
#endif

namespace Assets.Scripts
{
    public class Game
    {
        public class Objective
        {
            public bool Revealed;
            public string ObjectiveText;
            public bool Completed;
        }
        
        private static Game _instance;

        private Dictionary<int, Objective> _objectives;
        private Dictionary<int, string> _failMessages;
        private bool _paused;
        
        private const string RevealObjectiveSound = "cnote.gpw";

        public static Game Instance
        {
            get { return _instance ?? (_instance = new Game()); }
        }

        public string LevelName { get; set; }
        public string GamePath { get; set; }
        public bool IntroPlayed { get; set; }
        public string MapFileName { get; set; }
        public string PlayerName { get; set; }
        public Font Font { get; private set; }
        
        public bool Paused
        {
            get { return _paused; }
            set
            {
                if (_paused == value)
                {
                    return;
                }

                _paused = value;
                Time.timeScale = _paused ? 0f : 1f;

                AudioSource[] audioSources = Object.FindObjectsOfType<AudioSource>();
                for (int i = 0; i < audioSources.Length; ++i)
                {
                    if (_paused)
                    {
                        audioSources[i].Pause();
                    }
                    else
                    {
                        audioSources[i].UnPause();
                    }
                }
            }
        }

        public List<Objective> GetVisibleObjectives()
        {
            List<Objective> visibleObjectives = new List<Objective>();

            foreach (Objective objective in _objectives.Values)
            {
                if (objective.Revealed)
                {
                    visibleObjectives.Add(objective);
                }
            }

            return visibleObjectives;
        }

        public void FailAllObjectives(int secondsToGameOver, int failMessageIndex)
        {
            if (!_failMessages.TryGetValue(failMessageIndex, out string failMessage))
            {
                Debug.LogError($"Tried to retrieve failure message at index '{failMessageIndex}', but no message exists at this index.");
            }
            
            if (SceneRoot.Instance != null)
            {
                SceneRoot.Instance.StartCoroutine(GameOverAsync(secondsToGameOver, failMessage));
            }
        }

        private IEnumerator GameOverAsync(int secondsToGameOver, string failMessage)
        {
            yield return new WaitForSeconds(secondsToGameOver);

            // TODO: Show game over screen.
            Debug.Log("Fail Message: " + failMessage);
        }

        public void RevealObjective(int objectiveIndex)
        {
            if (!_objectives.TryGetValue(objectiveIndex, out Objective objective))
            {
                Debug.LogError($"Tried to reveal objective at index '{objectiveIndex}', but no objective exists at this index.");
                return;
            }
            
            SceneRoot.Instance.PlayUiSound(RevealObjectiveSound);

            objective.Revealed = true;
        }

        public void CompleteObjective(int objectiveIndex)
        {
            if (!_objectives.TryGetValue(objectiveIndex, out Objective objective))
            {
                Debug.LogError($"Tried to complete objective at index '{objectiveIndex}', but no objective exists at this index.");
                return;
            }

            objective.Completed = true;
        }

        private void ParseObjectives(string objectiveFilePath)
        {
            if (!VirtualFilesystem.Instance.FileExists(objectiveFilePath))
            {
                return;
            }

            using (FastBinaryReader br = VirtualFilesystem.Instance.GetFileStream(objectiveFilePath))
            {
                string objectiveText = Encoding.ASCII.GetString(br.Data, (int)br.Position, br.Length - (int)br.Position);
                objectiveText = objectiveText.Replace("\r", "");
                string[] lines = objectiveText.Split('\n');

                if (lines.Length == 0)
                {
                    return;
                }

                int lineIndex = 0;
                string currentLine = lines[0];

                // Read objectives until blank line.
                while (currentLine.Length > 0)
                {
                    Objective objective = new Objective();
                    if (currentLine.StartsWith("(HIDDEN)"))
                    {
                        currentLine = currentLine.Remove(0, 8);
                    }
                    else
                    {
                        objective.Revealed = true;
                    }

                    objective.ObjectiveText = currentLine;
                    _objectives.Add(lineIndex + 1, objective);

                    if (lineIndex + 1 == lines.Length)
                    {
                        return;
                    }

                    currentLine = lines[++lineIndex];
                }

                if (lineIndex + 2 >= lines.Length)
                {
                    return;
                }

                // Read failure conditions.
                int failureIndex = 1;
                if (lines[lineIndex + 1] == "(failure)")
                {
                    lineIndex += 2;
                    currentLine = lines[lineIndex];
                    while (currentLine.Length > 0)
                    {
                        _failMessages.Add(failureIndex++, currentLine);

                        if (lineIndex + 1 == lines.Length)
                        {
                            return;
                        }

                        currentLine = lines[++lineIndex];
                    }
                }
            }
        }

        public void SetActiveMission(MsnMissionParser.MissonDefinition missionDefinition)
        {
            _objectives.Clear();
            _failMessages.Clear();
            MapFileName = null;

            if (missionDefinition == null)
            {
                return;
            }

            MapFileName = missionDefinition.LevelMapFilePath;
            ParseObjectives(missionDefinition.ObjectiveFilePath);
        }

        public void Destroy()
        {
            _objectives.Clear();
            _failMessages.Clear();
            _objectives = null;
            _failMessages = null;
            _instance = null;
        }

        private Game()
        {
            _objectives = new Dictionary<int, Objective>();
            _failMessages = new Dictionary<int, string>();
            PlayerName = "Unnamed";

            Font = Resources.Load<Font>("Fonts/LEE_____");

#if !UNITY_EDITOR
            string gameExeDir = Path.Combine(Application.dataPath, "../..");
            if (File.Exists(Path.Combine(gameExeDir, "i76.exe")))
            {
                GamePath = gameExeDir;
            }
            else
            {
                Debug.LogError("Game path not found.");
            }
#endif
        }
    }
}
