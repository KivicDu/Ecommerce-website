/* ================================================================
   site.js — Global JS
   - Toast notification
   - Add to cart (AJAX)
   - Cart badge update
   ================================================================ */

// ── TOAST ─────────────────────────────────────────────────────────
function showToast(message, type = "success", duration = 3000) {
  let container = document.getElementById("toast-container");
  if (!container) {
    container = document.createElement("div");
    container.id = "toast-container";
    document.body.appendChild(container);
  }

  const icons = {
    success: "fa-check-circle",
    error: "fa-exclamation-circle",
    info: "fa-info-circle",
  };
  const toast = document.createElement("div");
  toast.className = `toast-item ${type}`;
  toast.innerHTML = `<i class="fas ${icons[type] || icons.info}"></i><span>${message}</span>`;
  container.appendChild(toast);

  setTimeout(() => {
    toast.style.transition = "opacity .3s, transform .3s";
    toast.style.opacity = "0";
    toast.style.transform = "translateX(120%)";
    setTimeout(() => toast.remove(), 300);
  }, duration);
}

// ── ADD TO CART ───────────────────────────────────────────────────
async function addToCart(productId, event, variantId = null, quantity = 1) {
  if (event) {
    event.preventDefault();
    event.stopPropagation();
  }

  const btn = event?.currentTarget;
  if (btn) {
    btn.disabled = true;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
  }

  try {
    const res = await fetch("/Cart/AddItem", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        RequestVerificationToken: getAntiForgeryToken(),
      },
      body: JSON.stringify({ productId, variantId, quantity }),
    });
    const data = await res.json();

    if (data.success) {
      showToast("Đã thêm vào giỏ hàng! 🛒", "success");
      updateCartBadge(data.cartCount);
    } else {
      showToast(data.message || "Không thể thêm vào giỏ hàng", "error");
    }
  } catch {
    showToast("Lỗi kết nối. Thử lại nhé!", "error");
  } finally {
    if (btn) {
      btn.disabled = false;
      btn.innerHTML = '<i class="fas fa-cart-plus"></i>';
    }
  }
}

// ── WISHLIST TOGGLE ───────────────────────────────────────────────
async function toggleWishlist(productId, btn) {
  try {
    const res = await fetch("/Wishlist/Toggle", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        RequestVerificationToken: getAntiForgeryToken(),
      },
      body: JSON.stringify({ productId }),
    });
    const data = await res.json();

    if (data.success) {
      const icon = btn?.querySelector("i");
      if (icon) {
        icon.classList.toggle("fas", data.added);
        icon.classList.toggle("far", !data.added);
      }
      if (btn) {
        btn.classList.toggle("active", data.added);
        btn.classList.toggle("text-danger", data.added);
        btn.classList.toggle("border-danger", data.added);
        btn.title = data.added ? "Bỏ yêu thích" : "Thêm yêu thích";
      }
      // Cập nhật wishlist badge trên header (nếu có)
      const wishlistBadge =
        document.getElementById("wishlistBadge") ||
        document.querySelector(".wishlist-badge");
      if (wishlistBadge) {
        const current = parseInt(wishlistBadge.textContent) || 0;
        const next = data.added ? current + 1 : Math.max(0, current - 1);
        wishlistBadge.textContent = next;
        wishlistBadge.style.display = next > 0 ? "flex" : "none";
      }
      showToast(
        data.added ? "Đã thêm vào yêu thích ❤️" : "Đã xóa khỏi yêu thích",
        "info",
      );
    } else if (data.requireLogin) {
      window.location.href = "/Auth/Login";
    }
  } catch {
    showToast("Lỗi kết nối", "error");
  }
}

// ── CART BADGE ────────────────────────────────────────────────────
function updateCartBadge(count) {
  const badge =
    document.getElementById("cartBadge") ||
    document.querySelector(".cart-badge");
  if (!badge) return;

  if (count > 0) {
    badge.textContent = count;
    badge.style.display = "flex";
  } else {
    badge.style.display = "none";
  }
}

// ── ANTI-FORGERY TOKEN ────────────────────────────────────────────
function getAntiForgeryToken() {
  return (
    document.querySelector('input[name="__RequestVerificationToken"]')?.value ??
    ""
  );
}

// ── INIT ──────────────────────────────────────────────────────────
document.addEventListener("DOMContentLoaded", () => {
  // Bootstrap tooltips
  document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach((el) => {
    new bootstrap.Tooltip(el, { trigger: "hover" });
  });

  // Auto-dismiss alerts sau 5s
  document.querySelectorAll(".alert:not(.alert-permanent)").forEach((alert) => {
    setTimeout(() => {
      const bsAlert = new bootstrap.Alert(alert);
      bsAlert.close();
    }, 5000);
  });
});
