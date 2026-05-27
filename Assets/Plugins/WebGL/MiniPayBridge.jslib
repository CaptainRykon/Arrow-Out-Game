mergeInto(LibraryManager.library, {

    MiniPayBridge_Initialize: function () {

        if (window.__miniPayBridgeCallbackInstalled) {
            return;
        }

        window.__miniPayBridgeCallbackInstalled = true;

        window.addEventListener("message", function (event) {

            const data = event.data;

            if (!data || data.type !== "UNITY_CALLBACK" || !data.method) {
                return;
            }

            const value =
                typeof data.value === "string"
                    ? data.value
                    : JSON.stringify(
                        data.value !== undefined && data.value !== null
                            ? data.value
                            : ""
                    );

            try {

                if (typeof SendMessage === "function") {
                    SendMessage("MiniPayBridge", data.method, value);
                    return;
                }

                if (window.unityInstance && typeof window.unityInstance.SendMessage === "function") {
                    window.unityInstance.SendMessage("MiniPayBridge", data.method, value);
                    return;
                }

                console.warn("MiniPayBridge callback target missing", data.method);

            } catch (error) {

                console.error("MiniPayBridge callback failed", error);
            }
        });
    },

    // =========================================================
    // INTERNAL HELPERS
    // =========================================================

    MiniPayBridge_PostMessage: function (typePtr, payloadPtr) {

        const type =
            UTF8ToString(typePtr);

        const payload =
            payloadPtr
                ? UTF8ToString(payloadPtr)
                : "";

        let parsedPayload = null;

        try {

            parsedPayload =
                payload
                    ? JSON.parse(payload)
                    : null;

        } catch {

            parsedPayload = payload;
        }

        const targetWindow =
            window.parent && window.parent !== window
                ? window.parent
                : window;

        targetWindow.postMessage(
            {
                type: type,
                payload: parsedPayload
            },
            "*"
        );
    },

    // =========================================================
    // BOOTSTRAP
    // =========================================================

    MiniPayBridge_RequestBootstrap: function () {

        const targetWindow =
            window.parent && window.parent !== window
                ? window.parent
                : window;

        targetWindow.postMessage(
            {
                type:
                    "MINIPAY_BOOTSTRAP"
            },
            "*"
        );
    },

    // =========================================================
    // SYNC USER STATE
    // =========================================================

    MiniPayBridge_SyncUserState: function (
        snapshotPtr
    ) {

        const snapshot =
            UTF8ToString(snapshotPtr);

        let parsed = {};

        try {

            parsed =
                JSON.parse(snapshot);

        } catch {

            console.error(
                "Failed to parse snapshot"
            );

            return;
        }

        const targetWindow =
            window.parent && window.parent !== window
                ? window.parent
                : window;

        targetWindow.postMessage(
            {
                type:
                    "MINIPAY_SYNC_USER_STATE",

                payload:
                    parsed
            },
            "*"
        );
    },

    // =========================================================
    // PURCHASE GAME ENTRY
    // =========================================================

    MiniPayBridge_PurchaseGame: function (
        tokenPtr
    ) {

        const token =
            tokenPtr
                ? UTF8ToString(tokenPtr)
                : "USDT";

        const targetWindow =
            window.parent && window.parent !== window
                ? window.parent
                : window;

        targetWindow.postMessage(
            {
                type:
                    "MINIPAY_PURCHASE_GAME",

                payload: {
                    token: token
                }
            },
            "*"
        );
    },

    // =========================================================
    // BUY HINTS
    // =========================================================

    MiniPayBridge_BuyHints: function (
        amount,
        tokenPtr
    ) {

        const token =
            tokenPtr
                ? UTF8ToString(tokenPtr)
                : "USDT";

        const targetWindow =
            window.parent && window.parent !== window
                ? window.parent
                : window;

        targetWindow.postMessage(
            {
                type:
                    "MINIPAY_BUY_HINTS",

                payload: {
                    amount: amount,
                    token: token
                }
            },
            "*"
        );
    },

    // =========================================================
    // BUY REVIVE
    // =========================================================

    MiniPayBridge_BuyRevive: function (
        amount,
        tokenPtr,
        modePtr
    ) {

        const token =
            tokenPtr
                ? UTF8ToString(tokenPtr)
                : "USDT";
        const mode =
            modePtr
                ? UTF8ToString(modePtr)
                : "classic";

        const targetWindow =
            window.parent && window.parent !== window
                ? window.parent
                : window;

        targetWindow.postMessage(
            {
                type:
                    "MINIPAY_BUY_REVIVE",

                payload: {
                    amount: amount,
                    token: token,
                    mode: mode
                }
            },
            "*"
        );
    },

    MiniPayBridge_BuyLives: function (
        amount,
        tokenPtr
    ) {
        LibraryManager.library.MiniPayBridge_BuyRevive(
            amount,
            tokenPtr,
            0
        );
    },

    // =========================================================
    // SUBMIT SCORE
    // =========================================================

    MiniPayBridge_SubmitChallengeScore: function (
        payloadPtr
    ) {

        const payload =
            UTF8ToString(payloadPtr);

        let parsed = {};

        try {

            parsed =
                JSON.parse(payload);

        } catch {

            console.error(
                "Invalid score payload"
            );

            return;
        }

        const targetWindow =
            window.parent && window.parent !== window
                ? window.parent
                : window;

        targetWindow.postMessage(
            {
                type:
                    "MINIPAY_SUBMIT_SCORE",

                payload:
                    parsed
            },
            "*"
        );
    },

    // =========================================================
    // GET LEADERBOARD
    // =========================================================

    MiniPayBridge_RequestLeaderboard: function (
        payloadPtr
    ) {

        const payload =
            UTF8ToString(payloadPtr);

        let parsed = {};

        try {

            parsed =
                JSON.parse(payload);

        } catch {

            console.error(
                "Invalid leaderboard payload"
            );

            return;
        }

        const targetWindow =
            window.parent && window.parent !== window
                ? window.parent
                : window;

        targetWindow.postMessage(
            {
                type:
                    "MINIPAY_GET_LEADERBOARD",

                payload:
                    parsed
            },
            "*"
        );
    }
});
