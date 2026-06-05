const loginView = document.querySelector("#loginView");
const dashboardView = document.querySelector("#dashboardView");
const loginForm = document.querySelector("#loginForm");
const loginMessage = document.querySelector("#loginMessage");
const printButton = document.querySelector("#printButton");
const printMessage = document.querySelector("#printMessage");

loginForm.addEventListener("submit", async (event) => {
  event.preventDefault();

  const phone = loginForm.phone.value.trim();
  const password = loginForm.password.value;

  if (!/^\d{10}$/.test(phone)) {
    showMessage(loginMessage, "Enter exactly 10 digits for the phone number.", true);
    return;
  }

  const submitButton = loginForm.querySelector("button[type='submit']");
  submitButton.disabled = true;
  showMessage(loginMessage, "Checking credentials...");

  try {
    const response = await fetch("/api/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ phone, password }),
    });
    const result = await response.json();

    if (!response.ok || !result.success) {
      showMessage(loginMessage, result.message || "Login failed.", true);
      return;
    }

    loginView.classList.add("hidden");
    dashboardView.classList.remove("hidden");
  } catch (error) {
    showMessage(loginMessage, "Could not reach the Go backend.", true);
  } finally {
    submitButton.disabled = false;
  }
});

printButton.addEventListener("click", async () => {
  printButton.disabled = true;
  showMessage(printMessage, "Sending print command...");

  try {
    const response = await fetch("/api/print", { method: "POST" });
    const result = await response.json();

    if (!response.ok || !result.success) {
      showMessage(printMessage, result.message || "Print failed.", true);
      return;
    }

    showMessage(printMessage, result.message);
    window.setTimeout(() => showMessage(printMessage, ""), 3500);
  } catch (error) {
    showMessage(printMessage, "Could not reach the Go backend.", true);
  } finally {
    printButton.disabled = false;
  }
});

function showMessage(element, text, isError = false) {
  element.textContent = text;
  element.classList.toggle("error", isError);
}
