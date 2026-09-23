using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace Be_Stiff.old
{
	internal class MusicManager : GameComponent
	{
		private SoundEffectInstance songInstance;

		private SoundEffect song;

		private ContentManager content;

		public MusicManager(Game game)
			: base(game)
		{
			content = new CaseInsensitiveContentManager(game.Services);
		}

		public void LoadNewSong(string songName)
		{
			song = content.Load<SoundEffect>(songName);
			songInstance = song.CreateInstance();
			songInstance.IsLooped = true;
			songInstance.Volume = (float)Globals.OptionMusicVolume / 10f;
		}

		public void PlaySong()
		{
			if (songInstance.State != SoundState.Playing)
			{
				songInstance.Play();
			}
		}

		public void StopSong()
		{
			if (songInstance.State != SoundState.Stopped)
			{
				songInstance.Stop();
			}
		}

		public void PauseSong()
		{
			if (songInstance.State != SoundState.Paused)
			{
				songInstance.Pause();
			}
		}
	}
}
