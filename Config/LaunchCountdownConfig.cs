﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LaunchCountDown.Common;
using UnityEngine;

namespace LaunchCountDown.Config
{
    class ConfigInfo
    {
        private readonly RectWrapper _wrapper = new RectWrapper();

        private bool _isDebug;

        private bool _isSoundEnabled;

        private string _soundSet;

        private bool _engineControl;

        private bool _abortExecuted;

        private float _scale = 1f;

        private Dictionary<Guid, string[]> _sequences = new Dictionary<Guid, string[]>();// { { Guid.Empty, new[] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty } } };

        private readonly string _configPath; 

        public ConfigInfo(string path)
        {
            _configPath = path;
        }

        internal bool IsDebug
        {
            get { return _isDebug; }
            set
            {
                if (_isDebug == value) return;
                _isDebug = value;
                Save();
                OnChangedExecute(ConfigProperties.IsDebug);
            }
        }

        internal bool IsSoundEnabled
        {
            get { return _isSoundEnabled; }
            set
            {
                if (_isSoundEnabled == value) return;
                _isSoundEnabled = value;
                Save();
                OnChangedExecute(ConfigProperties.IsSoundEnabled);
            }
        }

        internal string SoundSet
        {
            get { return _soundSet; }
            set
            {
                if (_soundSet == value) return;
                _soundSet = value;
                Save();
                OnChangedExecute(ConfigProperties.SoundSet);
            }
        }

        internal Dictionary<Guid, string[]> Sequences
        {
            get { return _sequences; }
            set { _sequences = value; }
        }

        internal Rect WindowPosition { get; set; }

        internal bool EngineControl
        {
            get { return _engineControl; }
            set
            {
                if (_engineControl == value) return;
                _engineControl = value;
                Save();
                OnChangedExecute(ConfigProperties.EngineControl);
            }
        }

        internal bool AbortExecuted
        {
            get { return _abortExecuted; }
            set
            {
                if (_abortExecuted == value) return;

                _abortExecuted = value;
                Save();
                OnChangedExecute(ConfigProperties.AbortExecuted);
            }
        }

        internal float Scale
        {
            get { return _scale; }
            set
            {
                if (_scale == value) return;

                _scale = (float)Math.Round(value, 1);
                Save();
                OnChangedExecute(ConfigProperties.Scale);
            }
        }

        internal bool IsLoaded { get; private set; }

        #region Events
        [field: NonSerialized]
        internal event EventHandler<ConfigEventArgs> OnChanged;

        private void OnChangedExecute(ConfigProperties data)
        {
            if (OnChanged != null)
            {
                OnChanged(this, new ConfigEventArgs(data));
            }
        }
        #endregion

        public void Load()
        {
            try
            {
                var node = ConfigNode.Load(_configPath);

                if (node == null) throw new NullReferenceException("Node not exist");

                if (node.HasValue("isDebug"))
                {
                    IsDebug = bool.Parse(node.GetValue("isDebug"));
                }

                if (node.HasValue("soundEnabled"))
                {
                    IsSoundEnabled = bool.Parse(node.GetValue("soundEnabled"));
                }

                if (node.HasValue("engineControl"))
                {
                    EngineControl = bool.Parse(node.GetValue("engineControl"));
                }

                if (node.HasValue("abort"))
                {
                    AbortExecuted = bool.Parse(node.GetValue("abort"));
                }

                if (node.HasValue("scale"))
                {
                    Scale = float.Parse(node.GetValue("scale"));
                }

                if (node.HasValue("soundSet"))
                {
                    SoundSet = node.GetValue("soundSet");
                }

                if (node.HasValue("position"))
                {
                    WindowPosition = _wrapper.ToRect(node.GetValue("position"));
                    Debug.LogWarning("Position is" + WindowPosition);
                }

                if (node.HasNode("sequence"))
                {
                    var sequences = node.GetNodes("sequence");

                    Sequences.Clear();

                    foreach (var sequence in sequences)
                    {
                        Sequences.Add(new Guid(sequence.GetValue("id")), sequence.GetValue("stages").Split(','));
                    }
                }

                IsLoaded = true;
            }
            catch (Exception ex)
            {
                Debug.LogError("Cannot load config");
                Debug.LogException(ex);
                IsLoaded = false;
            }
        }

        public void Save()
        {
            var node = new ConfigNode();

            node.AddValue("isDebug", IsDebug);
            node.AddValue("soundEnabled", IsSoundEnabled);
            node.AddValue("soundSet", SoundSet);

            _wrapper.FromRect(WindowPosition);

            node.AddValue("position", _wrapper);
            node.AddValue("engineControl", EngineControl);
            node.AddValue("abort", AbortExecuted);
            node.AddValue("scale", Scale);

            foreach (var sequence in Sequences)
            {
                var seqNode = node.AddNode("sequence");

                seqNode.AddValue("id", sequence.Key);

                var value = sequence.Value.Aggregate((x, y) => string.Format("{0},{1}", x, y));

                seqNode.AddValue("stages", value);
            }

            node.Save(_configPath);
        }
    }


    class RectWrapper
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        internal Rect ToRect(string data)
        {
            var items = data.Split(',');

            if (!items.Any() || items.Count() < 4) return new Rect(0, 0, 459, 120);

            X = float.Parse(items[0]);
            Y = float.Parse(items[1]);
            Width = float.Parse(items[2]);
            Height = float.Parse(items[3]);

            return new Rect(X, Y, Width, Height);
        }

        internal void FromRect(Rect source)
        {
            X = source.x;
            Y = source.y;
            Width = source.width;
            Height = source.height;
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}", X, Y, Width, Height);
        }
    }

    class LaunchCountdownConfig
    {
        private static LaunchCountdownConfig _config;

        private readonly string _configPath;

        private LaunchCountdownConfig()
        {
            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var configDir = Path.Combine(currentDir, "config");

            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }

            _configPath = Path.Combine(configDir, "lcdex.cfg");

            Info = new ConfigInfo(_configPath);

            Info.Load();
        }

        internal static LaunchCountdownConfig Instance
        {
            get { return _config ?? (_config = new LaunchCountdownConfig()); }
        }

        public ConfigInfo Info { get; private set; }

    }
}


