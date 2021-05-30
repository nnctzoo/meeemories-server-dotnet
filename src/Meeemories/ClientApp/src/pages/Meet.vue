<template>
    <section id="meet">
        <div class="meet__remote" ref="remotes"></div>
        <div class="meet__local" :class="{'meet__local--float':joined}">
            <video ref="local" playsinline muted @click="openSettings"></video>
            <div class="meet__local__toolbar">
                <label><input type="radio" name="mic" value="true" v-model="audioEnable" /><span class="text">マイク ON</span></label>
                <label><input type="radio" name="mic" value="false" v-model="audioEnable" /><span class="text">マイク OFF</span></label>
                <button @click="joinRoom" class="btn btn-primary" v-if="!joined">入室</button>
                <button @click="leaveRoom" class="btn btn-primary" v-else>退室</button>
            </div>
            <div class="meet__local__settings" v-if="showSettins">
                <div class="meet__local__settings__body">
                    <dl>
                        <dt>カメラ</dt>
                        <dd>
                            <select @change="changeDevice" v-model="selectedVideoDevice">
                                <option v-for="dev in videoDevices" :key="dev.deviceId" :value="dev.deviceId">{{dev.label}}</option>
                            </select>
                        </dd>
                    </dl>
                    <dl>
                        <dt>マイク</dt>
                        <dd>
                            <select @change="changeDevice" v-model="selectedAudioDevice">
                                <option v-for="dev in audioDevices" :key="dev.deviceId" :value="dev.deviceId">{{dev.label}}</option>
                            </select>
                        </dd>
                    </dl>
                    <button class="btn" @click="closeSettings">とじる</button>
                </div>
            </div>
        </div>
        <button class="popup__close" @click="back">
            <i class="material-icons">arrow_back</i>
        </button>
    </section>
</template>
<script>
    import { getInputDevices, openLocalStream, join, leave, replaceStream } from '../meet.js'
    export default {
        data() {
            return {
                videoDevices: [],
                audioDevices: [],
                videoEnable: true,
                audioEnable: false,
                selectedVideoDevice: null,
                selectedAudioDevice: null,
                joined: false,
                showSettins: false,
            }
        },
        computed: {
        },
        methods: {
            toggleVideoEnable() {
                this.videoEnable = !this.videoEnable;
                this.changeDevice();
            },
            async changeDevice() {
                const localStream = await openLocalStream(
                    this.selectedVideoDevice, this.videoEnable,
                    this.selectedAudioDevice, this.audioEnable);

                this.stopLocalStream();

                this.$refs.local.srcObject = localStream;
                this.$refs.local.play();

                replaceStream(localStream);
            },
            stopLocalStream() {
                if (this.$refs.local && this.$refs.local.srcObject) {
                    this.$refs.local.pause();
                    this.$refs.local.srcObject.getTracks().forEach(track => track.stop());
                    this.$refs.local.srcObject = null;
                }
            },
            joinRoom() {
                if (this.$refs.local.srcObject) {
                    join({
                        localStream: this.$refs.local.srcObject,
                        onOpenRoom: () => {
                            console.log('open room');
                            this.joined = true;
                        },
                        onCloseRoom: () => {
                            console.log('close room');
                            this.joined = false;
                        },
                        onPeerJoin: id => console.log(`peer ${id} is joined`),
                        onPeerLeave: id => {
                            console.log(`peer ${id} is leaved`)
                            const remoteVideo = this.$refs.querySelector(
                                `[data-peer-id="${peerId}"]`
                            );
                            remoteVideo.srcObject.getTracks().forEach(track => track.stop());
                            remoteVideo.srcObject = null;
                            remoteVideo.remove();
                        },
                        onOpenRemoteStream: stream => {
                            const newVideo = document.createElement('video');
                            newVideo.srcObject = stream;
                            newVideo.playsInline = true;
                            newVideo.setAttribute('data-peer-id', stream.peerId);
                            newVideo.addEventListener('click', this.selectRemote.bind(this));
                            this.$refs.remotes.append(newVideo);
                            newVideo.play().catch(console.error);
                            this.masonry();
                        }
                    })
                }
            },
            leaveRoom() {
                leave();
            },
            selectRemote(ev) {
                for (let active of this.$refs.remotes.querySelectorAll('.active')) {
                    active.classList.remove('active');
                }
                ev.target.classList.add('active');
                this.masonry();
            },
            masonry() {
                const vw = window.innerWidth;
                const vh = window.innerHeight;
                const vmin = Math.min(vw, vh);
                const vmax = Math.max(vw, vh);
                const dv = vmax - vmin;
                const count = this.$refs.remotes.children.length - 1;
                let s = 200;
                for (let i = 1; i < 10; i++) {
                    s = ~~(Math.min(dv, vmin) / i);
                    if (i * ~~(Math.max(dv, vmin) / s) > count)
                        break;
                }

                let active = this.$refs.remotes.querySelector('video.active');
                if (!active) active = this.$refs.remotes.querySelector('video');
                active.classList.add('active');
                active.style.top = '0';
                active.style.left = '0';
                active.style.width = '100vmin';
                active.style.height = '100vmin';

                const elements = this.$refs.remotes.querySelectorAll('video:not(.active)');
                
                for (let index = 0; index < elements.length; index++) {
                    const el = elements[index];
                    el.style.width = `${s}px`;
                    el.style.height = `${s}px`;
                    if (vmin == vw) {
                        const m = (dv - (s * ~~(dv / s))) / 2;
                        const wc = ~~(vw / s);
                        const y = ~~(index / wc);
                        const x = index % wc;
                        el.style.left = `${s * x}px`;
                        el.style.top = `${s * y + vmin + m}px`;
                    }
                    else {
                        const m = (dv - (s * ~~(dv / s))) / 2;
                        const wc = ~~((vw - vmin) / s);
                        const y = ~~(index / wc);
                        const x = index % wc;
                        el.style.left = `${s * x + vmin + m}px`;
                        el.style.top = `${s * y}px`;
                    }
                }
            },
            back() {
                this.leaveRoom();
                this.stopLocalStream();
                this.$router.push('/');
            },
            async openSettings() {
                const { videos, audios } = await getInputDevices();

                this.videoDevices.splice(0, this.videoDevices.length, ...videos);
                this.audioDevices.splice(0, this.audioDevices.length, ...audios);

                if (this.selectedVideoDevice == null && videos.length > 0)
                    this.selectedVideoDevice = (videos.find(dev => dev.deviceId == 'default') || videos[0]).deviceId;

                if (this.selectedAudioDevice == null && audios.length > 0)
                    this.selectedAudioDevice = (audios.find(dev => dev.deviceId == 'default') || audios[0]).deviceId;

                this.showSettins = true;
            },
            closeSettings() {
                this.showSettins = false;
            }
        },
        watch: {
            audioEnable() {
                this.changeDevice();
            }
        },
        async mounted() {
            if (!await this.$actions.authorize()) {
                this.$router.push('/login');
            }
            else {
                await this.changeDevice();
            }
        }
    }
</script>