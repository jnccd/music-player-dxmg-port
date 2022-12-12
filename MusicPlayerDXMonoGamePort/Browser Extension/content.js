var buttons = [];

window.ontransitionend = function () {
  if (buttons.length == 0) {
    
	/// song download button
    addButton("Song Download", function(){
		var video = document.querySelector( 'video' );
		if (video) 
		{
			video.pause();
			var downloadstring = window.location.href + '±' + video.currentTime;
			download('MusicPlayer.PlayRequest', downloadstring);
		}
	});
	
	/// video download button
    addButton("Video Download", function(){
		var video = document.querySelector('video');
		if (video) 
		{
			video.pause();
			var downloadstring = window.location.href;
			download('MusicPlayer.VideoDownloadRequest', downloadstring);
		}
	});
    
    /// view thumbnail button
    addButton("View Thumbnail", function(){
        var videoID = window.location.search.split('v=')[1].split('&')[0];
		window.open(`https://img.youtube.com/vi/${videoID}/maxresdefault.jpg`);
	});
	
    /// properly resize ma bois
    //updateButtonSize();
	//window.addEventListener('resize', updateButtonSize);
	
	/// Remove pesky official download button advertisement
	var adDownload = document.getElementsByClassName('style-scope ytd-download-button-renderer')[0]
	adDownload.parentNode.removeChild(adDownload)
    
	console.log('MusicPlayer buttons added!');
  }
}

function addButton(text, clickEvent)
{
    /// prepare button object
    var b = document.createElement('ytd-button-renderer');
	b.className = 'style-scope ytd-menu-renderer';
	b.setAttribute('is-icon-button', '');
	b.style.width='125px';
	b.addEventListener("click", function() {
      clickEvent();
    });
    
    /// add button to document
    var container = document.getElementsByClassName('item style-scope ytd-watch-metadata')[0];
	container.appendChild(b);
    
    /// post add adjustments
    b.children[0].append(document.createTextNode(text));
	b.children[0].className = "yt-spec-button-shape-next yt-spec-button-shape-next--tonal yt-spec-button-shape-next--mono yt-spec-button-shape-next--size-m yt-spec-button-shape-next--icon-leading";
    
    buttons[buttons.length] = b;
}

function updateButtonSize() {
    var p = document.getElementById('primary-inner');
    
    for (var i = 0; i < buttons.length; i++) {
        buttons[i].style.width = (p.offsetWidth/buttons.length - 8)+'px';
        buttons[i].children[0].style.width = (p.offsetWidth/buttons.length - 8)+'px';
    }
}

function download(filename, text) {
    var pom = document.createElement('a');
    pom.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text));
    pom.setAttribute('download', filename);

    if (document.createEvent) {
        var event = document.createEvent('MouseEvents');
        event.initEvent('click', true, true);
        pom.dispatchEvent(event);
    }
    else {
        pom.click();
    }
}