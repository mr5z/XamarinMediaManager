﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AVFoundation;
using MediaManager.Media;
using MediaManager.Platforms.Apple;
using MediaManager.Platforms.Apple.Media;
using MediaManager.Platforms.Apple.Notifications;
using MediaManager.Platforms.Apple.Playback;
using MediaManager.Playback;
using MediaManager.Queue;
using MediaManager.Volume;

namespace MediaManager
{
    public abstract class AppleMediaManagerBase<TMediaPlayer> : MediaManagerBase where TMediaPlayer : AppleMediaPlayer, IMediaPlayer<AVQueuePlayer>, new()
    {
        private IMediaPlayer _mediaPlayer;
        public override IMediaPlayer MediaPlayer
        {
            get
            {
                if (_mediaPlayer == null)
                {
                    _mediaPlayer = new TMediaPlayer();
                }
                return _mediaPlayer;
            }
            set => SetProperty(ref _mediaPlayer, value);
        }

        public AppleMediaPlayer AppleMediaPlayer => (AppleMediaPlayer)MediaPlayer;

        private IMediaExtractor _mediaExtractor;
        public override IMediaExtractor MediaExtractor
        {
            get
            {
                if (_mediaExtractor == null)
                {
                    _mediaExtractor = new AppleMediaExtractor();
                }
                return _mediaExtractor;
            }
            set => SetProperty(ref _mediaExtractor, value);
        }

        private IVolumeManager _volumeManager;
        public override IVolumeManager VolumeManager
        {
            get
            {
                if (_volumeManager == null)
                    _volumeManager = new VolumeManager();
                return _volumeManager;
            }
            set => SetProperty(ref _volumeManager, value);
        }

        private INotificationManager _notificationManager;
        public override INotificationManager NotificationManager
        {
            get
            {
                if (_notificationManager == null)
                    _notificationManager = new NotificationManager();

                return _notificationManager;
            }
            set => SetProperty(ref _notificationManager, value);
        }

        /*public override MediaPlayerState State
        {
            get
            {
                return AppleMediaPlayer.Player.TimeControlStatus.ToMediaPlayerState();
            }
        }*/

        public override TimeSpan Position
        {
            get
            {
                if (AppleMediaPlayer?.Player?.CurrentItem == null)
                {
                    return TimeSpan.Zero;
                }
                return TimeSpan.FromSeconds(AppleMediaPlayer.Player.CurrentTime.Seconds);
            }
        }

        public override TimeSpan Duration
        {
            get
            {
                if (AppleMediaPlayer?.Player?.CurrentItem == null)
                {
                    return TimeSpan.Zero;
                }
                if (double.IsNaN(AppleMediaPlayer.Player.CurrentItem.Duration.Seconds))
                {
                    return TimeSpan.Zero;
                }
                return TimeSpan.FromSeconds(AppleMediaPlayer.Player.CurrentItem.Duration.Seconds);
            }
        }

        public override float Speed
        {
            get
            {
                if (AppleMediaPlayer?.Player != null)
                    return AppleMediaPlayer.Player.Rate;
                return 0.0f;
            }
            set
            {
                if (AppleMediaPlayer?.Player != null)
                    AppleMediaPlayer.Player.Rate = value;
            }
        }

        public override void Init()
        {
            IsInitialized = true;
        }

        public override Task Pause()
        {
            return MediaPlayer.Pause();
        }

        public override async Task Play(IMediaItem mediaItem)
        {
            var mediaItemToPlay = await AddMediaItemsToQueue(new List<IMediaItem> { mediaItem }, true);

            await MediaPlayer.Play(mediaItemToPlay);
        }

        public override async Task<IMediaItem> Play(string uri)
        {
            var mediaItem = await MediaExtractor.CreateMediaItem(uri);
            var mediaItemToPlay = await AddMediaItemsToQueue(new List<IMediaItem> { mediaItem }, true);

            await MediaPlayer.Play(mediaItemToPlay);
            return mediaItem;
        }

        public override async Task Play(IEnumerable<IMediaItem> items)
        {
            var mediaItemToPlay = await AddMediaItemsToQueue(items, true);

            await MediaPlayer.Play(mediaItemToPlay);
        }

        public override async Task<IEnumerable<IMediaItem>> Play(IEnumerable<string> items)
        {
            var mediaItems = new List<IMediaItem>();
            foreach (var uri in items)
            {
                mediaItems.Add(await MediaExtractor.CreateMediaItem(uri));
            }

            var mediaItemToPlay = await AddMediaItemsToQueue(mediaItems, true);
            await MediaPlayer.Play(mediaItemToPlay);
            return MediaQueue;
        }

        public override async Task<IMediaItem> Play(FileInfo file)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<IMediaItem>> Play(DirectoryInfo directoryInfo)
        {
            throw new NotImplementedException();
        }

        public override Task Play()
        {
            return MediaPlayer.Play();
        }

        public override Task SeekTo(TimeSpan position)
        {
            return MediaPlayer.SeekTo(position);
        }

        public override Task Stop()
        {
            return MediaPlayer.Stop();
        }

        public override RepeatMode RepeatMode
        {
            get
            {
                return MediaPlayer.RepeatMode;
            }
            set
            {
                MediaPlayer.RepeatMode = value;
            }
        }

        public override ShuffleMode ShuffleMode
        {
            get
            {
                return MediaQueue.ShuffleMode;
            }
            set
            {
                MediaQueue.ShuffleMode = value;
            }
        }
    }
}
