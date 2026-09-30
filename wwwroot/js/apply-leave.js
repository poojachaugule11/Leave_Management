(function () {
    const from = document.getElementById('FromDate');
    const to = document.getElementById('ToDate');
    const total = document.getElementById('TotalDays');
    if (!from || !to || !total) return;

    const MS_PER_DAY = 24 * 60 * 60 * 1000;

    function recalc() {
        if (!from.value || !to.value) { total.value = 0; return; }

        // Parse as UTC so daylight-saving changes cannot skew the result.
        const a = Date.parse(from.value + 'T00:00:00Z');
        const b = Date.parse(to.value + 'T00:00:00Z');

        total.value = (isNaN(a) || isNaN(b) || a > b)
            ? 0
            : Math.round((b - a) / MS_PER_DAY) + 1;
    }

    // Keep the To date picker from going earlier than From.
    from.addEventListener('change', function () {
        to.min = from.value || '';
        recalc();
    });
    to.addEventListener('change', recalc);

    to.min = from.value || '';
    recalc();
})();
