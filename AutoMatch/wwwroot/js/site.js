// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
// Write your JavaScript code.

// Contact form submission
document.addEventListener('DOMContentLoaded', function () {
    const contactForm = document.getElementById('contactForm');

    if (contactForm) {
        contactForm.addEventListener('submit', async function (e) {
            e.preventDefault();

            const submitBtn = document.getElementById('submitBtn');
            const formMessage = document.getElementById('formMessage');
            const originalBtnText = submitBtn.textContent;

            // Get form data
            const formData = {
                fullName: document.getElementById('fullName').value.trim(),
                email: document.getElementById('email').value.trim(),
                topic: document.getElementById('topic').value.trim(),
                message: document.getElementById('message').value.trim()
            };

            // Basic client-side validation
            if (!formData.fullName || !formData.email || !formData.topic || !formData.message) {
                showMessage('Please fill in all fields.', 'error');
                return;
            }

            // Email format validation
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(formData.email)) {
                showMessage('Please enter a valid email address.', 'error');
                return;
            }

            // Disable button and show loading state
            submitBtn.disabled = true;
            submitBtn.textContent = 'Sending...';
            formMessage.style.display = 'none';

            try {
                const response = await fetch('/Home/SubmitContactForm', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(formData)
                });

                const result = await response.json();

                if (response.ok && result.success) {
                    showMessage(result.message, 'success');
                    contactForm.reset();
                } else {
                    showMessage(result.message || 'Failed to send message. Please try again.', 'error');
                }
            } catch (error) {
                console.error('Error:', error);
                showMessage('An error occurred. Please try again later.', 'error');
            } finally {
                submitBtn.disabled = false;
                submitBtn.textContent = originalBtnText;
            }
        });

        function showMessage(message, type) {
            const formMessage = document.getElementById('formMessage');
            formMessage.textContent = message;
            formMessage.style.display = 'block';

            if (type === 'success') {
                formMessage.style.backgroundColor = '#d4edda';
                formMessage.style.color = '#155724';
                formMessage.style.border = '1px solid #c3e6cb';
            } else {
                formMessage.style.backgroundColor = '#f8d7da';
                formMessage.style.color = '#721c24';
                formMessage.style.border = '1px solid #f5c6cb';
            }

            // Auto-hide success messages after 5 seconds
            if (type === 'success') {
                setTimeout(() => {
                    formMessage.style.display = 'none';
                }, 5000);
            }
        }
    }

    // ============================================
    // USER DROPDOWN TOGGLE
    // ============================================
    const userBadge = document.querySelector('.user-badge');
    const userDropdown = document.querySelector('.user-dropdown');

    if (userBadge && userDropdown) {
        // Toggle dropdown on badge click
        const userMenuContainer = document.querySelector('.user-menu-container');

        userBadge.addEventListener('click', function (e) {
            e.stopPropagation();
            userDropdown.classList.toggle('show');
            userMenuContainer.classList.toggle('open');  // <--- ADD THIS
        });


        // Close dropdown when clicking outside
        document.addEventListener('click', function (e) {
            if (!e.target.closest('.user-menu-container')) {
                userDropdown.classList.remove('show');
                userMenuContainer.classList.remove('open');  // <--- ADD THIS
            }
        });

        // Close dropdown when pressing Escape key
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && userDropdown.classList.contains('show')) {
                userDropdown.classList.remove('show');
                userMenuContainer.classList.remove('open'); // <--- ADD THIS
            }
        });


        // Prevent dropdown from closing when clicking inside it
        userDropdown.addEventListener('click', function (e) {
            e.stopPropagation();
        });
    }
});