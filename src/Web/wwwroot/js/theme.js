(function () {
    var STORAGE_KEY = "eshop-theme";
    var PREFERENCES = ["light", "dark", "system"];

    function getPreference() {
        try {
            var stored = localStorage.getItem(STORAGE_KEY);
            if (PREFERENCES.indexOf(stored) !== -1) {
                return stored;
            }
        } catch (e) {
            // localStorage can be unavailable in private browsing.
        }

        return "system";
    }

    function systemTheme() {
        if (window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches) {
            return "dark";
        }

        return "light";
    }

    function resolve(preference) {
        return preference === "system" ? systemTheme() : preference;
    }

    function syncControls(preference) {
        var current = preference || document.documentElement.getAttribute("data-theme-preference") || getPreference();
        var buttons = document.querySelectorAll("[data-theme-value]");
        for (var i = 0; i < buttons.length; i++) {
            var selected = buttons[i].getAttribute("data-theme-value") === current;
            buttons[i].setAttribute("aria-pressed", selected ? "true" : "false");
            buttons[i].classList.toggle("is-active", selected);
        }
    }

    function apply(preference) {
        var resolved = resolve(preference);
        var root = document.documentElement;
        root.setAttribute("data-theme", resolved);
        root.setAttribute("data-theme-preference", preference);
        root.style.colorScheme = resolved;
        syncControls(preference);
    }

    function setPreference(preference) {
        if (PREFERENCES.indexOf(preference) === -1) {
            return;
        }

        try {
            localStorage.setItem(STORAGE_KEY, preference);
        } catch (e) {
            // Ignore storage failures and still apply the choice for this visit.
        }

        apply(preference);
    }

    document.addEventListener("click", function (event) {
        var target = event.target;
        if (!target || !target.closest) {
            return;
        }

        var button = target.closest("[data-theme-value]");
        if (!button) {
            return;
        }

        setPreference(button.getAttribute("data-theme-value"));
    });

    if (window.matchMedia) {
        var media = window.matchMedia("(prefers-color-scheme: dark)");
        var onChange = function () {
            if (getPreference() === "system") {
                apply("system");
            }
        };

        if (media.addEventListener) {
            media.addEventListener("change", onChange);
        } else if (media.addListener) {
            media.addListener(onChange);
        }
    }

    window.eshopTheme = {
        setPreference: setPreference,
        getPreference: getPreference,
        syncControls: function () {
            syncControls(getPreference());
        }
    };

    apply(getPreference());

    document.addEventListener("DOMContentLoaded", function () {
        syncControls(getPreference());
    });
})();
