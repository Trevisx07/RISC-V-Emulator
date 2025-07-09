// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Page Load Slide-In
window.addEventListener('load', () => {
    document.querySelector('.vlsi-container').classList.add('slide-in');
});

// Ripple Effect on Buttons
document.querySelectorAll('.vlsi-btn').forEach(btn => {
    btn.addEventListener('click', (e) => {
        const ripple = document.createElement('span');
        ripple.classList.add('ripple');
        const rect = btn.getBoundingClientRect();
        const size = Math.max(rect.width, rect.height);
        ripple.style.width = ripple.style.height = `${size}px`;
        ripple.style.left = `${e.clientX - rect.left - size / 2}px`;
        ripple.style.top = `${e.clientY - rect.top - size / 2}px`;
        btn.appendChild(ripple);
        setTimeout(() => ripple.remove(), 600);
    });
});

// Real-Time Input Validation
const codeInput = document.querySelector('.code-input');
if (codeInput) {
    codeInput.addEventListener('input', (e) => {
        const lines = e.target.value.split('\n');
        const errorDiv = document.querySelector('.alert');
        let errorMessage = '';

        lines.forEach((line, index) => {
            const trimmed = line.trim();
            if (trimmed && !/^(addi|slli|lw|sw|jal|beq|add|sub|and|or|xor)\s+x\d+\s*(,?\s*x\d+)*\s*(-?\d+)?/.test(trimmed)) {
                errorMessage += `Line ${index + 1}: Invalid instruction format\n`;
            }
        });

        if (errorMessage) {
            if (!errorDiv) {
                const newErrorDiv = document.createElement('div');
                newErrorDiv.className = 'alert alert-danger';
                newErrorDiv.innerHTML = `<strong>Error:</strong> <pre>${errorMessage}</pre>`;
                codeInput.parentElement.insertBefore(newErrorDiv, codeInput.nextSibling);
            } else {
                errorDiv.innerHTML = `<strong>Error:</strong> <pre>${errorMessage}</pre>`;
            }
        } else if (errorDiv) {
            errorDiv.remove();
        }
    });
}

// Output Refresh Animation
function refreshOutput() {
    const outputCards = document.querySelectorAll('.output-card');
    outputCards.forEach(card => {
        card.classList.add('refresh');
        setTimeout(() => card.classList.remove('refresh'), 500);
    });
}

// Trigger refresh on form submission (Run/Step)
document.querySelectorAll('form').forEach(form => {
    form.addEventListener('submit', () => {
        setTimeout(refreshOutput, 100); // Delay to sync with server response
    });
});