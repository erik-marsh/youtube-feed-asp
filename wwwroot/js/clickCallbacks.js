const delete_video = (videoId) => {
    let row = document.getElementById(videoId);
    row.remove();

    fetch(`/Video/videos/${videoId}`, { method: "DELETE" });
};