const delete_video = (videoId) => {
    let row = document.getElementById(videoId);
    row.remove();

    fetch(`/Video/api/videos/${videoId}`, { method: "DELETE" });
};

const update_channel_subscriptions = (channelId) => {
    let res = fetch(`/Video/api/channels/${channelId}`, { method: "PUT" });
    console.log(res);
};

const subscribe_to_channel = () => {
    var inputField = document.getElementById("add_subscription_field");
    let channelId = inputField.value;

    fetch(`/Video/api/channels/${channelId}`, { method: "POST" });
};