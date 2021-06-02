import Peer from 'skyway-js';

export async function getInputDevices() {
    const videos = [];
    const audios = [];
    const devices = await navigator.mediaDevices.enumerateDevices();
    for (let { kind, deviceId, label } of devices) {
        switch (kind) {
            case 'videoinput':
                videos.push({ deviceId, label });
                break;
            case 'audioinput':
                audios.push({ deviceId, label });
                break;
        }
    }
    return { videos, audios };
}

export async function openLocalStream(videoDeviceId, videoEnabled, audioDeviceId, audioEnabled) {
    let localStream;
    try {
        localStream = await navigator.mediaDevices.getUserMedia({
            video: videoDeviceId ? { deviceId: { exact: videoDeviceId } } : true,
            audio: audioDeviceId ? { deviceId: { exact: audioDeviceId } } : true,
        })
    } catch (err) {
        console.error(err);
    }

    if (localStream) {
        localStream.getVideoTracks().forEach(track => { track.enabled = videoEnabled });
        localStream.getAudioTracks().forEach(track => { track.enabled = audioEnabled });
    }

    return localStream;
}

let _peer = null;
function lazyPeer(token) {
    return new Promise(function (resolve, reject) {
        if (_peer && _peer.open) {
            resolve(_peer);
        }
        else {
            fetch('/api/skyway', {
                method: 'post',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    token
                })
            }).then(response => {
                if (!response.ok) {
                    reject(response.statusText);
                }
                else {
                    response.json().then(credential => {
                        _peer = new Peer(credential.peerId, {
                            key: window.skywayKey,
                            credential,
                            debug: 3,
                        });
                        _peer.once('open', () => {
                            resolve(_peer);
                        });
                        _peer.once('error', () => {
                            _peer = null;
                            reject();
                        });
                        _peer.once('close', () => {
                            _peer = null;
                        })
                    }).catch(reject);
                }
            }).catch(reject);
        }
    })
}
const empty = function () { };
const roomId = 'live';
let room = null;
export function join({
    token,
    localStream,
    onOpenRoom = empty,
    onPeerJoin = empty,
    onOpenRemoteStream = empty,
    onPeerLeave = empty,
    onCloseRoom = empty
}) {
    lazyPeer(token)
        .then(function (peer) {
            room = peer.joinRoom(roomId, {
                mode: 'sfu',
                stream: localStream,
            });

            room.once('open', () => {
                onOpenRoom();
            });

            room.on('peerJoin', peerId => {
                onPeerJoin(peerId);
            });
            
            room.on('stream', stream => {
                onOpenRemoteStream(stream);
            });

            room.on('peerLeave', peerId => {
                onPeerLeave(peerId)
            });

            room.once('close', () => {
                room = null;
                onCloseRoom();
            });
        }).catch(function (err) {
            console.error(err);
            alert('ライブの接続に失敗しました。');
        })
}

export function replaceStream(localStream) {
    if (room) {
        room.replaceStream(localStream);
    }
}

export function leave() {
    if (room) {
        try {
            room.close();
        }
        catch (err) {
            console.error(err);
            alert('ライブの切断に失敗しました。');
        }
    }
}

