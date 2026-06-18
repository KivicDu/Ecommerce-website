/* ================================================================
   chat.js — AI Chatbot Widget Logic
   Giao tiếp với ChatController qua /Chat/Send
   ================================================================ */

let chatOpen = false;
let isSending = false;

// ── Toggle mo/dong chat ──────────────────────────────────────────
function toggleChat() {
  const win = document.getElementById("chatWindow");
  const iconO = document.querySelector(".chat-icon-open");
  const iconC = document.querySelector(".chat-icon-close");
  const dot = document.getElementById("chatDot");

  // Use DOM state as source of truth instead of variable
  chatOpen = win.classList.contains("d-none");

  if (chatOpen) {
    win.classList.remove("d-none");
    if (iconO) iconO.classList.add("d-none");
    if (iconC) iconC.classList.remove("d-none");
    if (dot) dot.style.display = "none";
    setTimeout(() => document.getElementById("chatInput")?.focus(), 100);
    scrollToBottom();
  } else {
    win.classList.add("d-none");
    if (iconO) iconO.classList.remove("d-none");
    if (iconC) iconC.classList.add("d-none");
  }
}

// ── Gui tin nhan ─────────────────────────────────────────────────
async function sendMessage() {
  const input = document.getElementById("chatInput");
  const msg = input.value.trim();
  if (!msg || isSending) return;

  input.value = "";
  appendMessage("user", msg);
  removeQuickReplies();
  showTyping();

  isSending = true;
  const sendBtn = document.getElementById("chatSendBtn");
  if (sendBtn) sendBtn.disabled = true;

  try {
    const res = await fetch("/Chat/Send", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ message: msg }),
    });
    const data = await res.json();
    hideTyping();

    // Xu ly phan hoi thong minh dua theo trang thai success tu Controller
    if (data.success) {
      appendMessage("assistant", data.message);
    } else {
      // Neu gap su co ve han muc (429) hoac ket noi, hien thi thong bao chi tiet tu server
      appendMessage("assistant", `⚠️ ${data.message}`);
    }
  } catch {
    hideTyping();
    appendMessage(
      "assistant",
      "Khong the ket noi. Kiem tra internet va thu lai nhe! 🙏",
    );
  } finally {
    isSending = false;
    if (sendBtn) sendBtn.disabled = false;
    input.focus();
  }
}

// ── Quick replies ─────────────────────────────────────────────────
function sendQuickReply(btn) {
  const text = btn.textContent.trim();
  const input = document.getElementById("chatInput");
  input.value = text;
  sendMessage();
}

function removeQuickReplies() {
  const el = document.getElementById("quickReplies");
  if (el) el.remove();
}

// ── Xoa lich su ──────────────────────────────────────────────────
async function clearChat() {
  if (!confirm("Xoa toan bo lich su hoi thoai?")) return;

  try {
    await fetch("/Chat/Clear", { method: "POST" });
  } catch {}

  const msgs = document.getElementById("chatMessages");
  msgs.innerHTML = `
    <div class="chat-msg assistant">
      <div class="msg-avatar"><i class="fas fa-robot"></i></div>
      <div class="msg-bubble">
        <p>Hoi thoai da duoc xoa. Minh san sang tu van lai cho ban! 😊</p>
      </div>
    </div>`;
}

// ── Them message vao DOM ──────────────────────────────────────────
function appendMessage(role, text) {
  const msgs = document.getElementById("chatMessages");

  // Parse markdown don gian: **bold**, *italic*, dau xuong dong
  const html = markdownToHtml(text);

  const avatar =
    role === "user"
      ? '<div class="msg-avatar"><i class="fas fa-user"></i></div>'
      : '<div class="msg-avatar"><i class="fas fa-robot"></i></div>';

  const div = document.createElement("div");
  div.className = `chat-msg ${role}`;
  div.innerHTML = `${role === "user" ? "" : avatar}
    <div class="msg-bubble">${html}</div>
    ${role === "user" ? avatar : ""}`;

  msgs.appendChild(div);
  scrollToBottom();
}

// ── Typing indicator ──────────────────────────────────────────────
function showTyping() {
  const msgs = document.getElementById("chatMessages");
  const el = document.createElement("div");
  el.className = "chat-msg assistant chat-typing";
  el.id = "typingIndicator";
  el.innerHTML = `
    <div class="msg-avatar"><i class="fas fa-robot"></i></div>
    <div class="msg-bubble">
      <div class="typing-dots">
        <span></span><span></span><span></span>
      </div>
    </div>`;
  msgs.appendChild(el);
  scrollToBottom();
}

function hideTyping() {
  document.getElementById("typingIndicator")?.remove();
}

// ── Scroll xuong cuoi ─────────────────────────────────────────────
function scrollToBottom() {
  const msgs = document.getElementById("chatMessages");
  if (msgs) msgs.scrollTop = msgs.scrollHeight;
}

// ── Markdown don gian → HTML ──────────────────────────────────────
function markdownToHtml(text) {
  return (
    text
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      // bold **text**
      .replace(/\*\*(.*?)\*\*/g, "<strong>$1</strong>")
      // italic *text*
      .replace(/\*(.*?)\*/g, "<em>$1</em>")
      // code `text`
      .replace(/`(.*?)`/g, "<code>$1</code>")
      // dong moi
      .replace(/\n/g, "<br>")
      // bullet - item
      .replace(/^- (.+)/gm, "• $1")
  );
}
