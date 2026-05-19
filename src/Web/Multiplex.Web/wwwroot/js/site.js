// FRAME — minimal vanilla JS for the prototype interactions
(function () {
  'use strict';

  // ── Filter chip toggle (cartelera, admin tables) ──
  window.setFilter = function (el, group, value) {
    if (!el) return;
    var parent = el.parentElement;
    parent.querySelectorAll('.filter-chip').forEach(function (c) { c.classList.remove('active'); });
    el.classList.add('active');
    var ev = new CustomEvent('filter:change', { detail: { group: group, value: value } });
    document.dispatchEvent(ev);
  };

  // ── Modal open/close (admin) ──
  document.addEventListener('click', function (e) {
    var openSel = e.target.closest('[data-modal-open]');
    if (openSel) {
      e.preventDefault();
      var id = openSel.getAttribute('data-modal-open');
      var m = document.getElementById(id);
      if (m) m.classList.add('open');
      return;
    }
    if (e.target.classList.contains('modal-overlay')) {
      e.target.classList.remove('open');
      return;
    }
    var closeSel = e.target.closest('[data-modal-close]');
    if (closeSel) {
      var overlay = closeSel.closest('.modal-overlay');
      if (overlay) overlay.classList.remove('open');
    }
  });

  // ── Seat map (sillas.cshtml) ──
  document.querySelectorAll('[data-seat]').forEach(function (seat) {
    seat.addEventListener('click', function () {
      if (seat.classList.contains('taken')) return;
      seat.classList.toggle('selected');
      var input = seat.querySelector('input[type=checkbox]');
      if (input) input.checked = seat.classList.contains('selected');
      updateSeatSummary();
    });
  });

  function updateSeatSummary() {
    var selected = document.querySelectorAll('[data-seat].selected');
    var countEl = document.getElementById('seatCount');
    var totalEl = document.getElementById('seatTotal');
    var pricePerSeat = parseFloat((countEl && countEl.dataset.price) || '0');
    var list = document.getElementById('seatList');
    if (countEl) countEl.textContent = selected.length;
    if (totalEl) totalEl.textContent = (selected.length * pricePerSeat).toLocaleString('es-CO', { maximumFractionDigits: 0 });
    if (list) {
      list.innerHTML = '';
      selected.forEach(function (s) {
        var li = document.createElement('li');
        li.textContent = s.dataset.label;
        list.appendChild(li);
      });
    }
  }

  // ── Confitería quantity controls ──
  document.querySelectorAll('[data-qty]').forEach(function (group) {
    var input = group.querySelector('input');
    var dec = group.querySelector('[data-qty-dec]');
    var inc = group.querySelector('[data-qty-inc]');
    function set(v) {
      v = Math.max(0, parseInt(v || '0', 10));
      input.value = v;
      input.dispatchEvent(new Event('change', { bubbles: true }));
    }
    if (dec) dec.addEventListener('click', function () { set(parseInt(input.value, 10) - 1); });
    if (inc) inc.addEventListener('click', function () { set(parseInt(input.value, 10) + 1); });
  });

  // ── FAQ collapse (membresia.cshtml) ──
  document.querySelectorAll('[data-faq-q]').forEach(function (q) {
    q.addEventListener('click', function () {
      var a = q.nextElementSibling;
      if (a) a.toggleAttribute('hidden');
      q.classList.toggle('open');
    });
  });

  // ── Pelicula tabs (sinopsis / elenco / reviews) ──
  document.querySelectorAll('[data-tabs]').forEach(function (group) {
    var tabs = group.querySelectorAll('[data-tab]');
    var panels = group.querySelectorAll('[data-panel]');
    tabs.forEach(function (t) {
      t.addEventListener('click', function () {
        tabs.forEach(function (x) { x.classList.remove('active'); });
        panels.forEach(function (p) { p.hidden = true; });
        t.classList.add('active');
        var target = group.querySelector('[data-panel="' + t.dataset.tab + '"]');
        if (target) target.hidden = false;
      });
    });
  });

  // ── Order history row expand ──
  document.querySelectorAll('[data-row-toggle]').forEach(function (row) {
    row.addEventListener('click', function () {
      var id = row.getAttribute('data-row-toggle');
      var det = document.getElementById('det-' + id);
      if (det) det.toggleAttribute('hidden');
    });
  });
})();
