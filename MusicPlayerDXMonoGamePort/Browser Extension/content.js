/**
 * Music Player browser extension - YouTube helper.
 *
 * Adds the "Song Download", "Video Download" and "View Thumbnail" buttons to the
 * watch page and hands the request over to the Music Player by downloading a small
 * trigger file that the desktop player watches for.
 *
 * The buttons are plain <button> elements styled with YouTube's own design tokens
 * (see styles.css) instead of hand built YouTube components. YouTube regularly
 * reworks its internal button markup (yt-button-shape and friends), which is what
 * used to turn these buttons into plain, unstyled text - a native button keeps
 * working no matter what YouTube does to its own components.
 */
(() => {
  'use strict';

  const BUTTON_CLASS = 'music-player-download-button';
  const BUTTON_ATTR = 'data-music-player-button';

  /// YouTube re-renders constantly, so coalesce the resulting sync requests.
  const SYNC_DELAY_MS = 150;
  const RESYNC_INTERVAL_MS = 1000;

  /// The Music Player splits the play request payload on this separator.
  const PAYLOAD_SEPARATOR = '\u00b1';

  let syncTimeoutId = null;

  /* ---------------------------------------------------------------------- *
   * Injected buttons
   * ---------------------------------------------------------------------- */

  function createButtons() {
    return [
      createButton('Song Download', 'Download the current song to the Music Player', onSongDownload),
      createButton('Video Download', 'Download this video to the Music Player', onVideoDownload),
      createButton('View Thumbnail', 'Open this video\'s thumbnail in a new tab', onViewThumbnail),
    ];
  }

  function createButton(label, tooltip, action) {
    const button = document.createElement('button');

    button.type = 'button';
    button.className = BUTTON_CLASS;
    button.setAttribute(BUTTON_ATTR, '');
    button.title = tooltip;
    button.setAttribute('aria-label', tooltip);
    button.textContent = label;

    button.addEventListener('click', (event) => {
      /// Keep YouTube's own click handlers on the metadata row out of the way.
      event.preventDefault();
      event.stopPropagation();

      try {
        action();
      } catch (error) {
        console.error('[Music Player] Button action failed.', error);
      }
    });

    return button;
  }

  function onSongDownload() {
    const video = getVideo();
    if (video) {
      video.pause();
    }

    /// <url> + separator + <playback position in seconds>
    download('MusicPlayer.PlayRequest', window.location.href + PAYLOAD_SEPARATOR + (video ? video.currentTime : 0));
  }

  function onVideoDownload() {
    const video = getVideo();
    if (video) {
      video.pause();
    }

    download('MusicPlayer.VideoDownloadRequest', window.location.href);
  }

  function onViewThumbnail() {
    const videoId = getVideoId();
    if (!videoId) {
      console.warn('[Music Player] Could not determine the video id.');
      return;
    }

    window.open('https://img.youtube.com/vi/' + videoId + '/maxresdefault.jpg', '_blank', 'noopener');
  }

  function getVideo() {
    return document.querySelector('#movie_player video')
      || document.querySelector('ytd-player video')
      || document.querySelector('video');
  }

  function getVideoId() {
    const url = new URL(window.location.href);

    const fromQuery = url.searchParams.get('v');
    if (fromQuery) {
      return fromQuery;
    }

    const fromPath = url.pathname.match(/^\/(?:shorts|live|embed)\/([\w-]+)/);
    return fromPath ? fromPath[1] : null;
  }

  /* ---------------------------------------------------------------------- *
   * Injection
   * ---------------------------------------------------------------------- */

  function findInsertTarget() {
    /// Prefer the real watch page over the miniplayer, which reuses the same component.
    const metadata = document.querySelector('#primary ytd-watch-metadata') || document.querySelector('ytd-watch-metadata');
    if (!metadata || metadata.closest('ytd-miniplayer')) {
      return null;
    }

    /// Current layout: the buttons sit in the top row, right next to the subscribe button.
    const topRow = metadata.querySelector('#top-row');
    if (topRow) {
      return topRow;
    }

    /// Layouts YouTube used before - better than not showing up at all.
    return metadata.querySelector('#owner') || metadata.querySelector('#above-the-fold');
  }

  function syncButtons() {
    syncTimeoutId = null;

    const target = findInsertTarget();
    if (!target) {
      return;
    }

    removeOfficialDownloadButton();

    /// Still mounted where we put it - nothing to do.
    if (target.querySelector('[' + BUTTON_ATTR + ']')) {
      return;
    }

    /// Drop leftovers of a row YouTube has thrown away in the meantime.
    document.querySelectorAll('[' + BUTTON_ATTR + ']').forEach((button) => {
      if (!target.contains(button)) {
        button.remove();
      }
    });

    createButtons().forEach((button) => target.appendChild(button));
    console.log('[Music Player] Buttons added!');
  }

  function scheduleSync() {
    if (syncTimeoutId !== null) {
      return;
    }

    syncTimeoutId = window.setTimeout(syncButtons, SYNC_DELAY_MS);
  }

  function removeOfficialDownloadButton() {
    /// The official download button is a redundant advertisement next to ours.
    document.querySelectorAll('ytd-download-button-renderer').forEach((button) => {
      if (button.parentNode) {
        button.parentNode.removeChild(button);
      }
    });
  }

  /* ---------------------------------------------------------------------- *
   * Hand-off to the Music Player
   * ---------------------------------------------------------------------- */

  function download(filename, text) {
    const anchor = document.createElement('a');

    anchor.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text));
    anchor.setAttribute('download', filename);
    anchor.style.display = 'none';

    /// Browsers only honour the download attribute for anchors inside the document.
    document.body.appendChild(anchor);

    if (typeof anchor.click === 'function') {
      anchor.click();
    } else {
      const event = document.createEvent('MouseEvents');
      event.initEvent('click', true, true);
      anchor.dispatchEvent(event);
    }

    window.setTimeout(() => anchor.remove(), 0);
  }

  /* ---------------------------------------------------------------------- *
   * Boot
   * ---------------------------------------------------------------------- */

  function start() {
    syncButtons();

    /// YouTube rebuilds its DOM on navigation, lazy rendering and layout changes.
    new MutationObserver(scheduleSync).observe(document.documentElement, { childList: true, subtree: true });

    window.addEventListener('yt-navigate-finish', scheduleSync);
    document.addEventListener('yt-navigate-finish', scheduleSync, true);
    document.addEventListener('yt-page-data-updated', scheduleSync, true);

    /// Safety net for re-renders that produce no mutation we can observe.
    window.setInterval(scheduleSync, RESYNC_INTERVAL_MS);
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', start, { once: true });
  } else {
    start();
  }
})();
