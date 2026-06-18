/* ==========================================================================
   site.js — HutechStore Premium Interactions
   "Quiet Luxury Meets Tech Innovation"
   ========================================================================== */

/* ── LUXURY EASING CURVES ────────────────────────────────────────  */
const EASE = {
  luxury:  'cubic-bezier(0.16, 1, 0.3, 1)',
  precise: 'cubic-bezier(0.25, 1, 0.5, 1)',
  spring:  'cubic-bezier(0.175, 0.885, 0.32, 1.275)'
};

/* ── TOAST CONTAINER SYSTEM ────────────────────────────────────── */
function showToast(message, type = "success", duration = 3000) {
  let container = document.getElementById("toast-container");
  if (!container) {
    container = document.createElement("div");
    container.id = "toast-container";
    document.body.appendChild(container);
  }

  const icons = {
    success: "fa-check-circle",
    error:   "fa-exclamation-circle",
    info:    "fa-info-circle"
  };

  const toast = document.createElement("div");
  toast.className = `toast-item ${type}`;
  toast.innerHTML = `<i class="fas ${icons[type] || icons.info}"></i><span>${message}</span>`;
  container.appendChild(toast);

  setTimeout(() => {
    toast.style.transition = `opacity 0.4s ${EASE.luxury}, transform 0.4s ${EASE.luxury}`;
    toast.style.opacity    = "0";
    toast.style.transform  = "translateY(12px) scale(0.96)";
    setTimeout(() => toast.remove(), 420);
  }, duration);
}

/* ── HARDWARE-RESPONSIVE ADD TO CART ─────────────────────────────  */
async function addToCart(productId, event, variantId = null, quantity = 1) {
  if (event) {
    event.preventDefault();
    event.stopPropagation();
  }

  const btn = event?.currentTarget;
  const originalHtml = btn ? btn.innerHTML : '';
  
  if (btn) {
    btn.disabled  = true;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
    btn.style.transform = 'scale(0.95)';
  }

  try {
    const res  = await fetch("/Cart/AddItem", {
      method:  "POST",
      headers: {
        "Content-Type":              "application/json",
        "RequestVerificationToken":  getAntiForgeryToken()
      },
      body: JSON.stringify({ productId, variantId, quantity })
    });
    const data = await res.json();

    if (data.success) {
      showToast("Đã thêm vào giỏ hàng", "success");
      updateCartBadge(data.cartCount);

      if (btn) {
        btn.style.transform = 'scale(1.08)';
        setTimeout(() => { btn.style.transform = ''; }, 200);
      }
    } else {
      showToast(data.message || "Không thể thêm vào giỏ hàng", "error");
    }
  } catch {
    showToast("Lỗi kết nối hệ thống", "error");
  } finally {
    if (btn) {
      btn.disabled  = false;
      btn.innerHTML = originalHtml || '<i class="fas fa-cart-plus"></i>';
      btn.style.transform = '';
    }
  }
}

/* ── WISHLIST TRANSITION MANAGER ─────────────────────────────────  */
async function toggleWishlist(productId, btn) {
  try {
    const res  = await fetch("/Wishlist/Toggle", {
      method:  "POST",
      headers: {
        "Content-Type":             "application/json",
        "RequestVerificationToken": getAntiForgeryToken()
      },
      body: JSON.stringify({ productId })
    });
    const data = await res.json();

    if (data.success) {
      const icon = btn?.querySelector("i");
      if (icon) {
        icon.classList.toggle("fas",  data.added);
        icon.classList.toggle("far", !data.added);
      }
      if (btn) {
        btn.classList.toggle("active",       data.added);
        btn.classList.toggle("text-danger",  data.added);
        btn.classList.toggle("border-danger",data.added);
        btn.title = data.added ? "Bỏ yêu thích" : "Thêm yêu thích";

        btn.style.transform = 'scale(1.2)';
        btn.style.transition = `transform 0.4s ${EASE.spring}`;
        setTimeout(() => { btn.style.transform = ''; }, 400);
      }

      const wishlistBadge = document.getElementById("wishlistBadge") || document.querySelector(".wishlist-badge");
      if (wishlistBadge) {
        const current = parseInt(wishlistBadge.textContent) || 0;
        const next    = data.added ? current + 1 : Math.max(0, current - 1);
        wishlistBadge.textContent   = next;
        wishlistBadge.style.display = next > 0 ? "flex" : "none";
      }

      showToast(data.added ? "Đã thêm vào mục yêu thích" : "Đã xóa khỏi mục yêu thích", data.added ? "success" : "info");
    } else if (data.requireLogin) {
      window.location.href = "/Auth/Login";
    }
  } catch {
    showToast("Lỗi kết nối", "error");
  }
}

/* ── INTERACTIVE APP CART COUNTER ────────────────────────────────  */
function updateCartBadge(count) {
  const badge = document.getElementById("cartCountBadge") || document.getElementById("cartBadge") || document.querySelector(".cart-badge");
  if (!badge) return;

  if (count > 0) {
    badge.textContent   = count;
    badge.style.display = "flex";

    badge.style.transform  = 'scale(1.35)';
    badge.style.transition = `transform 0.4s ${EASE.spring}`;
    setTimeout(() => { badge.style.transform = ''; }, 400);
  } else {
    badge.style.display = "none";
  }
}

function getAntiForgeryToken() {
  return document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? "";
}

/* ── CHROMATIC GRADIENT SCROLL STATE ─────────────────────────────  */
function initHeaderScroll() {
  const header = document.querySelector(".site-header");
  if (!header) return;

  let ticking = false;
  const onScroll = () => {
    if (!ticking) {
      requestAnimationFrame(() => {
        if (window.scrollY > 20) {
          header.classList.add("scrolled");
        } else {
          header.classList.remove("scrolled");
        }
        ticking = false;
      });
      ticking = true;
    }
  };

  window.addEventListener("scroll", onScroll, { passive: true });
}

/* ── STAGGERED REVEAL INERTIA ────────────────────────────────────  */
function initScrollReveal() {
  document.querySelectorAll('.row[data-stagger], .hs-why-grid, .hs-brands, [data-stagger]')
    .forEach(container => {
      const children = container.querySelectorAll(':scope > *, :scope > .col > .product-card, :scope > .col-md-3, :scope > .col-md-4, :scope > .col-md-6, :scope > .col-lg-3, :scope > .col-lg-4');
      children.forEach((el, i) => {
        if (!el.classList.contains('reveal')) {
          el.classList.add('reveal', 'from-bottom');
          el.style.transitionDelay = `${Math.min(i * 0.06, 0.3)}s`;
        }
      });
    });

  const observer = new IntersectionObserver(
    (entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          entry.target.classList.add('visible');
          observer.unobserve(entry.target);
        }
      });
    },
    { threshold: 0.05, rootMargin: '0px 0px -40px 0px' }
  );

  document.querySelectorAll('.reveal').forEach(el => observer.observe(el));
}

/* ── CINEMATIC DEEPMOTION PARALLAX ───────────────────────────────  */
function initParallax() {
  const hero = document.querySelector('.hs-hero');
  if (!hero || window.innerWidth < 1024) return;

  const heroImg  = hero.querySelector('.hs-hero-img');
  const heroGlow = hero.querySelector('.hs-hero-img-glow');
  let raf        = null;

  const onScroll = () => {
    if (raf) cancelAnimationFrame(raf);
    raf = requestAnimationFrame(() => {
      const scrollY = window.scrollY;
      if (hero.getBoundingClientRect().bottom > 0) {
        if (heroImg)  heroImg.style.transform  = `translateY(${scrollY * 0.06}px) translateZ(0)`;
        if (heroGlow) heroGlow.style.transform = `translateX(-50%) translateY(${scrollY * 0.1}px) translateZ(0)`;
      }
    });
  };

  window.addEventListener('scroll', onScroll, { passive: true });
}

function initStoryPanels() {
  const panels = document.querySelectorAll('.hs-story-panel');
  if (!panels.length) return;

  const observer = new IntersectionObserver(
    entries => {
      entries.forEach(entry => {
        const text = entry.target.querySelector('.hs-story-text');
        if (text) text.classList.toggle('active', entry.isIntersecting);
      });
    },
    { threshold: 0.5 }
  );

  panels.forEach(panel => observer.observe(panel));
}

/* ── PREMIUM LIQUID FILTER TRANSITIONS ───────────────────────────  */
function initFilterTabs() {
  const tabs = document.querySelectorAll('.hs-filter-tab');
  if (!tabs.length) return;

  tabs.forEach(tab => {
    tab.addEventListener('click', function() {
      tabs.forEach(t => t.classList.remove('active'));
      this.classList.add('active');

      const brand = this.dataset.brand || '';
      const cards = document.querySelectorAll('.product-card-col');

      cards.forEach((card, i) => {
        const shouldShow = !brand || card.dataset.brand === brand;

        if (shouldShow) {
          card.style.display   = '';
          card.style.opacity   = '0';
          card.style.transform = 'translateY(16px)';
          requestAnimationFrame(() => {
            card.style.transition = `opacity 0.5s ${EASE.luxury}, transform 0.5s ${EASE.luxury}`;
            card.style.opacity    = '1';
            card.style.transform  = 'translateY(0)';
          });
        } else {
          card.style.transition = `opacity 0.3s ${EASE.precise}, transform 0.3s ${EASE.precise}`;
          card.style.opacity    = '0';
          card.style.transform  = 'translateY(12px)';
          setTimeout(() => { card.style.display = 'none'; }, 300);
        }
      });
    });
  });
}

/* ── METALLIC AMBIENT CURSOR TRAIL ───────────────────────────────  */
function initCursorTrail() {
  if (window.innerWidth < 1024 || window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;

  const trail = document.createElement('div');
  trail.style.cssText = `
    position: fixed; pointer-events: none; z-index: 99999;
    width: 6px; height: 6px; border-radius: 50%;
    background: #8C6239;
    transform: translate(-50%, -50%);
    transition: opacity 0.3s var(--ease-luxury), transform 0.3s var(--ease-luxury), background 0.3s ease;
    opacity: 0;
  `;
  document.body.appendChild(trail);

  let mouseX = 0, mouseY = 0, trailX  = 0, trailY  = 0;

  document.addEventListener('mousemove', e => {
    mouseX = e.clientX;
    mouseY = e.clientY;
    trail.style.opacity = '1';
  });

  const animate = () => {
    trailX += (mouseX - trailX) * 0.15;
    trailY += (mouseY - trailY) * 0.15;
    trail.style.left = trailX + 'px';
    trail.style.top  = trailY + 'px';
    requestAnimationFrame(animate);
  };
  animate();

  document.addEventListener('mouseover', e => {
    if (e.target.matches('a, button, .btn, .product-card, .hs-filter-tab, input, select, textarea')) {
      trail.style.transform = 'translate(-50%, -50%) scale(4)';
      trail.style.background = 'rgba(140, 98, 57, 0.08)';
      trail.style.border = '1px solid rgba(140, 98, 57, 0.3)';
    } else {
      trail.style.transform = 'translate(-50%, -50%) scale(1)';
      trail.style.background = '#8C6239';
      trail.style.border = 'none';
    }
  });

  document.addEventListener('mouseleave', () => { trail.style.opacity = '0'; });
}

function initSmoothAnchors() {
  document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', e => {
      const targetId = anchor.getAttribute('href');
      if (targetId === '#') return;
      const target = document.querySelector(targetId);
      if (!target) return;
      e.preventDefault();
      target.scrollIntoView({ behavior: 'smooth', block: 'start' });
    });
  });
}

function initImageReveal() {
  const images = document.querySelectorAll('img[loading="lazy"]');
  const observer = new IntersectionObserver(entries => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        const img = entry.target;
        img.style.transition = `opacity 0.6s ${EASE.luxury}`;
        if (img.complete) {
          img.style.opacity = '1';
        } else {
          img.style.opacity    = '0';
          img.addEventListener('load', () => { img.style.opacity = '1'; }, { once: true });
          img.addEventListener('error', () => { img.style.opacity = '1'; }, { once: true });
        }
        observer.unobserve(img);
      }
    });
  }, { threshold: 0.05 });
  images.forEach(img => observer.observe(img));
}

function animateCounter(el, target, duration = 1500) {
  let start = null;
  const step = (timestamp) => {
    if (!start) start = timestamp;
    const progress = Math.min((timestamp - start) / duration, 1);
    const eased = 1 - Math.pow(1 - progress, 5); // easeOutQuint
    el.textContent = Math.round(target * eased).toLocaleString('vi-VN');
    if (progress < 1) requestAnimationFrame(step);
  };
  requestAnimationFrame(step);
}

function initCounters() {
  const counters = document.querySelectorAll('[data-counter]');
  const observer = new IntersectionObserver(entries => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        const el = entry.target;
        animateCounter(el, parseInt(el.dataset.counter, 10));
        observer.unobserve(el);
      }
    });
  }, { threshold: 0.2 });
  counters.forEach(el => observer.observe(el));
}

/* ── HIGH-END METALLIC CARD MATRICES (3D PARALLAX EFFECT) ──────  */
function initCardTilt() {
  if (window.innerWidth < 1024 || window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;

  document.querySelectorAll('.product-card').forEach(card => {
    const TILT = 3;
    card.addEventListener('mousemove', e => {
      const rect = card.getBoundingClientRect();
      const dx   = (e.clientX - (rect.left + rect.width / 2)) / (rect.width / 2);
      const dy   = (e.clientY - (rect.top + rect.height / 2)) / (rect.height / 2);

      card.style.transform = `translateY(-6px) rotateY(${dx * TILT}deg) rotateX(${-dy * TILT}deg) scale(1.005) translateZ(0)`;
    });

    card.addEventListener('mouseleave', () => {
      card.style.transform = '';
      card.style.transition = `transform 0.6s ${EASE.luxury}`;
    });
  });
}

function togglePwd(inputId, btn) {
  const input = document.getElementById(inputId);
  if (!input) return;
  const icon = btn?.querySelector('i');
  const isPrivate = input.type === 'password';
  input.type = isPrivate ? 'text' : 'password';
  if (icon) {
    icon.classList.toggle('fa-eye', !isPrivate);
    icon.classList.toggle('fa-eye-slash', isPrivate);
  }
}

function initScrollToTop() {
  const btn = document.createElement('button');
  btn.id        = 'scrollTopBtn';
  btn.innerHTML = '<i class="fas fa-chevron-up"></i>';
  document.body.appendChild(btn);

  window.addEventListener('scroll', () => {
    btn.classList.toggle('visible', window.scrollY > 400);
  }, { passive: true });

  btn.addEventListener('click', () => { window.scrollTo({ top: 0, behavior: 'smooth' }); });
}

document.addEventListener("DOMContentLoaded", () => {
  // Gracefully enforce custom hardware tracking matrices over newly initialized card elements
  const productCards = document.querySelectorAll('.product-card');
  productCards.forEach((card, index) => {
    // Inject waterfall inline timing values securely without DOM interruption
    const animationDelay = (index % 4) * 0.08;
    card.style.animation = `luxuryConsoleReveal 0.75s cubic-bezier(0.16, 1, 0.3, 1) both`;
    card.style.animationDelay = `${animationDelay}s`;
  });

  // Ensure tooltips map correctly over the premium layout structure
  document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
    if (window.bootstrap && bootstrap.Tooltip) {
      new bootstrap.Tooltip(el, { trigger: "hover" });
    }
  });
});

/* Interactive color swatches - CSS filter simulation */
function changeProductColor(dot, colorName) {
  const card = dot.closest('.product-card');
  if (!card) return;
  const img = card.querySelector('.product-img');
  if (!img) return;

  const dots = card.querySelectorAll('.color-dot');
  dots.forEach(d => d.classList.remove('active'));
  dot.classList.add('active');

  const c = colorName.toLowerCase();
  img.style.transition = 'filter 0.4s ease, transform 0.8s cubic-bezier(0.16, 1, 0.3, 1)';
  
  if (c.includes('đen') || c.includes('black') || c.includes('tối')) {
    img.style.filter = 'brightness(0.6) contrast(1.1) grayscale(0.2)';
  } else if (c.includes('trắng') || c.includes('white') || c.includes('bạc') || c.includes('silver')) {
    img.style.filter = 'brightness(1.15) contrast(0.95) saturate(0.9)';
  } else if (c.includes('sa mạc') || c.includes('desert') || c.includes('vàng') || c.includes('gold')) {
    img.style.filter = 'sepia(0.4) hue-rotate(-12deg) saturate(1.4) brightness(0.98)';
  } else if (c.includes('xanh') || c.includes('blue') || c.includes('mòng két')) {
    img.style.filter = 'hue-rotate(130deg) saturate(0.85) brightness(0.9)';
  } else if (c.includes('hồng') || c.includes('pink')) {
    img.style.filter = 'hue-rotate(290deg) saturate(0.9) brightness(1.05)';
  } else if (c.includes('tự nhiên') || c.includes('natural') || c.includes('xám') || c.includes('gray')) {
    img.style.filter = 'grayscale(0.4) brightness(0.95) contrast(1.05)';
  } else {
    img.style.filter = 'hue-rotate(45deg) saturate(1.1)';
  }
}

function resetProductColor(dot) {
  const card = dot.closest('.product-card');
  if (!card) return;
  const img = card.querySelector('.product-img');
  if (!img) return;

  const dots = card.querySelectorAll('.color-dot');
  dots.forEach(d => d.classList.remove('active'));
  img.style.filter = '';
}

document.addEventListener("DOMContentLoaded", () => {
  initHeaderScroll();
  initScrollReveal();
  initParallax();
  initStoryPanels();
  initFilterTabs();
  initSmoothAnchors();
  initImageReveal();
  initCounters();
  initCursorTrail();
  initCardTilt();
  initScrollToTop();
});