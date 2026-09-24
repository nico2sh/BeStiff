using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace Be_Stiff.Audio
{
	public class AudioManager : GameComponent
	{
		private struct MusicFadeEffect
		{
			public float SourceVolume;

			public float TargetVolume;

			private TimeSpan _time;

			private TimeSpan _duration;

			public MusicFadeEffect(float sourceVolume, float targetVolume, TimeSpan duration)
			{
				SourceVolume = sourceVolume;
				TargetVolume = targetVolume;
				_time = TimeSpan.Zero;
				_duration = duration;
			}

			public bool Update(TimeSpan time)
			{
				_time += time;
				if (_time >= _duration)
				{
					_time = _duration;
					return true;
				}
				return false;
			}

			public float GetVolume()
			{
				return MathHelper.Lerp(SourceVolume, TargetVolume, (float)_time.Ticks / (float)_duration.Ticks);
			}
		}

		private const int MaxSounds = 24;

		private ContentManager _content;

		private Dictionary<string, Song> _songs = new Dictionary<string, Song>();

		private Dictionary<string, SoundEffect> _sounds = new Dictionary<string, SoundEffect>();

		private Song _currentSong;

		private SoundEffectInstance[] _playingSounds = new SoundEffectInstance[24];

		// Which slots hold looping sounds (level machinery), so they can be
		// paused with gameplay and stopped when the level is left.
		private bool[] _isLoop = new bool[24];

		private bool _isMusicPaused;

		private bool _isFading;

		private MusicFadeEffect _fadeEffect;

		public string CurrentSong { get; private set; }

		public float MusicVolume
		{
			get
			{
				return MediaPlayer.Volume;
			}
			set
			{
				MediaPlayer.Volume = value;
			}
		}

		public float SoundVolume
		{
			get
			{
				return SoundEffect.MasterVolume;
			}
			set
			{
				SoundEffect.MasterVolume = value;
			}
		}

		public bool IsSongActive
		{
			get
			{
				if (_currentSong != null)
				{
					return MediaPlayer.State != MediaState.Stopped;
				}
				return false;
			}
		}

		public bool IsSongPaused
		{
			get
			{
				if (_currentSong != null)
				{
					return _isMusicPaused;
				}
				return false;
			}
		}

		public AudioManager(Game game)
			: base(game)
		{
			_content = new CaseInsensitiveContentManager(game.Content.ServiceProvider, game.Content.RootDirectory);
			MediaPlayer.MediaStateChanged += MediaPlayer_MediaStateChanged;
		}

		public AudioManager(Game game, string contentFolder)
			: base(game)
		{
			_content = new CaseInsensitiveContentManager(game.Content.ServiceProvider, contentFolder);
			MediaPlayer.MediaStateChanged += MediaPlayer_MediaStateChanged;
		}

		private void MediaPlayer_MediaStateChanged(object sender, EventArgs e)
		{
			if (_currentSong != null && MediaPlayer.State == MediaState.Stopped)
			{
				_currentSong = null;
				CurrentSong = null;
				_isMusicPaused = false;
			}
		}

		public void LoadSong(string songName)
		{
			LoadSong(songName, songName);
		}

		public void LoadSong(string songName, string songPath)
		{
			if (_songs.ContainsKey(songName))
			{
				throw new InvalidOperationException($"Song '{songName}' has already been loaded");
			}
			_songs.Add(songName, _content.Load<Song>(songPath));
		}

		public void LoadSound(string soundName)
		{
			LoadSound(soundName, soundName);
		}

		public void LoadSound(string soundName, string soundPath)
		{
			if (!_sounds.ContainsKey(soundName))
			{
				_sounds.Add(soundName, _content.Load<SoundEffect>(soundPath));
			}
		}

		public void UnloadContent()
		{
			_content.Unload();
		}

		public void PlaySong(string songName)
		{
			PlaySong(songName, loop: false);
		}

		public void PlaySong(string songName, bool loop)
		{
			if (CurrentSong != songName)
			{
				if (_currentSong != null)
				{
					MediaPlayer.Stop();
				}
				if (!_songs.TryGetValue(songName, out _currentSong))
				{
					throw new ArgumentException($"Song '{songName}' not found");
				}
				CurrentSong = songName;
				_isMusicPaused = false;
				MediaPlayer.IsRepeating = loop;
				MediaPlayer.Play(_currentSong);
				if (!base.Enabled)
				{
					MediaPlayer.Pause();
				}
			}
		}

		public void PauseSong()
		{
			if (_currentSong != null && !_isMusicPaused)
			{
				if (base.Enabled)
				{
					MediaPlayer.Pause();
				}
				_isMusicPaused = true;
			}
		}

		public void ResumeSong()
		{
			if (_currentSong != null && _isMusicPaused)
			{
				if (base.Enabled)
				{
					MediaPlayer.Resume();
				}
				_isMusicPaused = false;
			}
		}

		public void StopSong()
		{
			if (_currentSong != null && MediaPlayer.State != MediaState.Stopped)
			{
				MediaPlayer.Stop();
				_isMusicPaused = false;
			}
		}

		public int PlaySound(string soundName)
		{
			return PlaySound(soundName, 1f, 0f, 0f);
		}

		public int PlaySound(string soundName, float volume)
		{
			return PlaySound(soundName, volume, 0f, 0f);
		}

		public int PlaySound(string soundName, float volume, float pitch, float pan)
		{
			return PlaySound(soundName, volume, pitch, pan, loop: false);
		}

		private int PlaySound(string soundName, float volume, float pitch, float pan, bool loop)
		{
			if (!_sounds.TryGetValue(soundName, out var value))
			{
				throw new ArgumentException($"Sound '{soundName}' not found");
			}
			int availableSoundIndex = GetAvailableSoundIndex();
			if (availableSoundIndex != -1)
			{
				_playingSounds[availableSoundIndex] = value.CreateInstance();
				_playingSounds[availableSoundIndex].Volume = volume;
				_playingSounds[availableSoundIndex].Pitch = pitch;
				_playingSounds[availableSoundIndex].Pan = pan;
				_playingSounds[availableSoundIndex].IsLooped = loop; // must be set before Play()
				_isLoop[availableSoundIndex] = loop;
				_playingSounds[availableSoundIndex].Play();
				if (!base.Enabled)
				{
					_playingSounds[availableSoundIndex].Pause();
				}
			}
			return availableSoundIndex;
		}

		public int PlaySoundLoop(string soundName)
		{
			return PlaySoundLoop(soundName, 1f);
		}

		public int PlaySoundLoop(string soundName, float volume)
		{
			return PlaySound(soundName, volume, 0f, 0f, loop: true);
		}

		/// <summary>Pauses or resumes every looping sound (e.g. while gameplay is covered by a menu).</summary>
		public void PauseSoundLoops(bool paused)
		{
			for (int i = 0; i < _playingSounds.Length; i++)
			{
				if (!_isLoop[i] || _playingSounds[i] == null)
				{
					continue;
				}
				if (paused && _playingSounds[i].State == SoundState.Playing)
				{
					_playingSounds[i].Pause();
				}
				else if (!paused && _playingSounds[i].State == SoundState.Paused)
				{
					_playingSounds[i].Resume();
				}
			}
		}

		/// <summary>Stops every looping sound, leaving one-shots (e.g. menu sounds) playing.</summary>
		public void StopSoundLoops()
		{
			for (int i = 0; i < _playingSounds.Length; i++)
			{
				if (_isLoop[i] && _playingSounds[i] != null)
				{
					_playingSounds[i].Stop();
				}
			}
		}

		public void StopSoundLoop(int soundIndex)
		{
			if (soundIndex >= 0 && _playingSounds[soundIndex] != null && _playingSounds[soundIndex].State == SoundState.Playing)
			{
				_playingSounds[soundIndex].Stop();
			}
		}

		public void SoundLoopVolume(int soundIndex, float volume)
		{
			if (soundIndex >= 0 && _playingSounds[soundIndex] != null)
			{
				_playingSounds[soundIndex].Volume = volume;
			}
		}

		public void StopAllSounds()
		{
			for (int i = 0; i < _playingSounds.Length; i++)
			{
				if (_playingSounds[i] != null)
				{
					_playingSounds[i].Stop();
					_playingSounds[i].Dispose();
					_playingSounds[i] = null;
					_isLoop[i] = false;
				}
			}
		}

		public override void Update(GameTime gameTime)
		{
			for (int i = 0; i < _playingSounds.Length; i++)
			{
				if (_playingSounds[i] != null && _playingSounds[i].State == SoundState.Stopped)
				{
					_playingSounds[i].Dispose();
					_playingSounds[i] = null;
					_isLoop[i] = false;
				}
			}
			base.Update(gameTime);
		}

		protected override void OnEnabledChanged(object sender, EventArgs args)
		{
			if (base.Enabled)
			{
				for (int i = 0; i < _playingSounds.Length; i++)
				{
					if (_playingSounds[i] != null && _playingSounds[i].State == SoundState.Paused)
					{
						_playingSounds[i].Resume();
					}
				}
				if (!_isMusicPaused)
				{
					MediaPlayer.Resume();
				}
			}
			else
			{
				for (int j = 0; j < _playingSounds.Length; j++)
				{
					if (_playingSounds[j] != null && _playingSounds[j].State == SoundState.Playing)
					{
						_playingSounds[j].Pause();
					}
				}
				MediaPlayer.Pause();
			}
			base.OnEnabledChanged(sender, args);
		}

		private int GetAvailableSoundIndex()
		{
			for (int i = 0; i < _playingSounds.Length; i++)
			{
				if (_playingSounds[i] == null)
				{
					return i;
				}
			}
			return -1;
		}
	}
}
