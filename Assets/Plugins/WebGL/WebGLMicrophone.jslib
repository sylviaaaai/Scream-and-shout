mergeInto(LibraryManager.library, {
  WebGLMicrophone_Start: function () {
    var state = window.__webglMicrophoneState;
    if (state && state.started) {
      return;
    }

    state = window.__webglMicrophoneState = {
      started: true,
      volume: 0,
      stream: null,
      audioContext: null,
      analyser: null,
      data: null
    };

    if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
      console.warn("Browser microphone API is not available.");
      return;
    }

    navigator.mediaDevices.getUserMedia({ audio: true }).then(function (stream) {
      var AudioContextCtor = window.AudioContext || window.webkitAudioContext;
      var audioContext = new AudioContextCtor();
      var source = audioContext.createMediaStreamSource(stream);
      var analyser = audioContext.createAnalyser();
      analyser.fftSize = 256;
      source.connect(analyser);

      state.stream = stream;
      state.audioContext = audioContext;
      state.analyser = analyser;
      state.data = new Uint8Array(analyser.fftSize);

      var update = function () {
        if (!state.started || !state.analyser) {
          return;
        }

        state.analyser.getByteTimeDomainData(state.data);

        var sum = 0;
        for (var i = 0; i < state.data.length; i++) {
          var sample = (state.data[i] - 128) / 128;
          sum += sample * sample;
        }

        state.volume = Math.sqrt(sum / state.data.length);
        window.requestAnimationFrame(update);
      };

      update();
    }).catch(function (error) {
      console.warn("Microphone permission was denied or failed: " + error.message);
    });
  },

  WebGLMicrophone_GetVolume: function () {
    var state = window.__webglMicrophoneState;
    return state ? state.volume : 0;
  },

  WebGLMicrophone_Stop: function () {
    var state = window.__webglMicrophoneState;
    if (!state) {
      return;
    }

    state.started = false;

    if (state.stream) {
      state.stream.getTracks().forEach(function (track) {
        track.stop();
      });
    }

    if (state.audioContext) {
      state.audioContext.close();
    }

    window.__webglMicrophoneState = null;
  }
});
