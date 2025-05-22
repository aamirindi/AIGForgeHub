function validateInput(input) {
            let warningMessage = document.getElementById("warning");
            if (!/^\d$/.test(input.value)) {
                input.value = ''; // Clear invalid input
                // warningMessage.style.display = "block"; // Show warning
            } else {
                warningMessage.style.display = "none"; // Hide warning
                let nextInput = input.nextElementSibling;
                if (nextInput) nextInput.focus(); // Move to the next box
            }
        }

         function handleBackspace(event, input) {
            if (event.key === "Backspace") {
                input.value = ''; // Clear current input
                let prevInput = input.previousElementSibling;
                if (prevInput) prevInput.focus(); // Move back one-by-one
            }
        }




        function clearOTP() {
            document.querySelectorAll(".otp-input").forEach(input => input.value = '');
            document.querySelector(".otp-input").focus(); // Focus on first box after clearing
}

