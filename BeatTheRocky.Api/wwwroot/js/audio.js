// Web Audio API Sound Synthesizer for "Beat The Rocky"
// Generates arcade boxing sound effects dynamically without external audio file dependencies.

class SoundEffects {
    constructor() {
        this.ctx = null;
        this.enabled = true;
    }

    init() {
        if (!this.ctx) {
            const AudioContext = window.AudioContext || window.webkitAudioContext;
            this.ctx = new AudioContext();
        }
        if (this.ctx.state === 'suspended') {
            this.ctx.resume();
        }
    }

    toggleSound() {
        this.enabled = !this.enabled;
        return this.enabled;
    }

    // Realistic Coin Flip Whoosh & Clink
    playCoinFlip() {
        if (!this.enabled) return;
        this.init();

        const now = this.ctx.currentTime;
        // Rising whirling tones
        for (let i = 0; i < 5; i++) {
            const osc = this.ctx.createOscillator();
            const gain = this.ctx.createGain();
            osc.type = 'sine';
            osc.frequency.setValueAtTime(400 + i * 120, now + i * 0.1);
            gain.gain.setValueAtTime(0.08, now + i * 0.1);
            gain.gain.exponentialRampToValueAtTime(0.001, now + (i + 1) * 0.1);
            osc.connect(gain);
            gain.connect(this.ctx.destination);
            osc.start(now + i * 0.1);
            osc.stop(now + (i + 1) * 0.1);
        }
    }

    playCoinLand() {
        if (!this.enabled) return;
        this.init();
        const now = this.ctx.currentTime;

        // Metallic bell-like ring
        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();
        osc.type = 'triangle';
        osc.frequency.setValueAtTime(1400, now);
        osc.frequency.exponentialRampToValueAtTime(800, now + 0.35);

        gain.gain.setValueAtTime(0.3, now);
        gain.gain.exponentialRampToValueAtTime(0.001, now + 0.35);

        osc.connect(gain);
        gain.connect(this.ctx.destination);
        osc.start(now);
        osc.stop(now + 0.35);
    }

    // Heavy Punch Sound
    playPunch(isRocky = false) {
        if (!this.enabled) return;
        this.init();
        const now = this.ctx.currentTime;

        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();

        osc.type = isRocky ? 'sawtooth' : 'sine';
        osc.frequency.setValueAtTime(isRocky ? 180 : 240, now);
        osc.frequency.exponentialRampToValueAtTime(35, now + 0.18);

        gain.gain.setValueAtTime(0.4, now);
        gain.gain.exponentialRampToValueAtTime(0.01, now + 0.18);

        osc.connect(gain);
        gain.connect(this.ctx.destination);

        osc.start(now);
        osc.stop(now + 0.18);
    }

    // Boxing Ring Bell: "DING! DING! DING!"
    playBell() {
        if (!this.enabled) return;
        this.init();
        const now = this.ctx.currentTime;

        [0, 0.25, 0.5].forEach(timeOffset => {
            const osc = this.ctx.createOscillator();
            const gain = this.ctx.createGain();

            osc.type = 'sine';
            osc.frequency.setValueAtTime(880, now + timeOffset); // A5

            gain.gain.setValueAtTime(0.35, now + timeOffset);
            gain.gain.exponentialRampToValueAtTime(0.001, now + timeOffset + 0.6);

            osc.connect(gain);
            gain.connect(this.ctx.destination);

            osc.start(now + timeOffset);
            osc.stop(now + timeOffset + 0.6);
        });
    }

    // Knockout / Game Over Gong
    playKnockout() {
        if (!this.enabled) return;
        this.init();
        const now = this.ctx.currentTime;

        const osc1 = this.ctx.createOscillator();
        const osc2 = this.ctx.createOscillator();
        const gain = this.ctx.createGain();

        osc1.type = 'sawtooth';
        osc2.type = 'square';
        osc1.frequency.setValueAtTime(110, now);
        osc2.frequency.setValueAtTime(55, now);
        osc1.frequency.exponentialRampToValueAtTime(25, now + 0.8);
        osc2.frequency.exponentialRampToValueAtTime(20, now + 0.8);

        gain.gain.setValueAtTime(0.5, now);
        gain.gain.exponentialRampToValueAtTime(0.001, now + 0.85);

        osc1.connect(gain);
        osc2.connect(gain);
        gain.connect(this.ctx.destination);

        osc1.start(now);
        osc2.start(now);
        osc1.stop(now + 0.85);
        osc2.stop(now + 0.85);
    }
}

window.soundFx = new SoundEffects();
