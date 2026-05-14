mergeInto(LibraryManager.library, {
  MiniPayBridge_RequestBootstrap: function () {
    const target = window.parent || window;
    target.postMessage({
      type: "UNITY_MINIPAY_REQUEST_BOOTSTRAP"
    }, "*");
  },

  MiniPayBridge_SyncUserState: function (payloadPtr) {
    const target = window.parent || window;
    const payloadJson = UTF8ToString(payloadPtr);
    let payload = payloadJson;

    try {
      payload = JSON.parse(payloadJson);
    } catch (error) {
    }

    target.postMessage({
      type: "UNITY_MINIPAY_SYNC_USER_STATE",
      payload: payload
    }, "*");
  },

  MiniPayBridge_PurchaseGame: function () {
    const target = window.parent || window;
    target.postMessage({
      type: "UNITY_MINIPAY_PURCHASE_GAME"
    }, "*");
  },

  MiniPayBridge_BuyHints: function (payloadPtr) {
    const target = window.parent || window;
    const payloadJson = UTF8ToString(payloadPtr);
    let payload = payloadJson;

    try {
      payload = JSON.parse(payloadJson);
    } catch (error) {
    }

    target.postMessage({
      type: "UNITY_MINIPAY_BUY_HINTS",
      payload: payload
    }, "*");
  },

  MiniPayBridge_BuyLives: function (payloadPtr) {
    const target = window.parent || window;
    const payloadJson = UTF8ToString(payloadPtr);
    let payload = payloadJson;

    try {
      payload = JSON.parse(payloadJson);
    } catch (error) {
    }

    target.postMessage({
      type: "UNITY_MINIPAY_BUY_LIVES",
      payload: payload
    }, "*");
  },

  MiniPayBridge_SubmitChallengeResult: function (payloadPtr) {
    const target = window.parent || window;
    const payloadJson = UTF8ToString(payloadPtr);
    let payload = payloadJson;

    try {
      payload = JSON.parse(payloadJson);
    } catch (error) {
    }

    target.postMessage({
      type: "UNITY_MINIPAY_SUBMIT_CHALLENGE_RESULT",
      payload: payload
    }, "*");
  },

  MiniPayBridge_RequestChallengeLeaderboard: function (payloadPtr) {
    const target = window.parent || window;
    const payloadJson = UTF8ToString(payloadPtr);
    let payload = payloadJson;

    try {
      payload = JSON.parse(payloadJson);
    } catch (error) {
    }

    target.postMessage({
      type: "UNITY_MINIPAY_REQUEST_CHALLENGE_LEADERBOARD",
      payload: payload
    }, "*");
  }
});
