document.addEventListener("DOMContentLoaded", function () {
    try {
        var amountInput = document.getElementById('amount');
        var termInput = document.getElementById('term');

        if (amountInput) {
            amountInput.addEventListener('input', function () {
                try {
                    updateAmountValue(this.value);
                } catch (error) {
                    console.error('Error updating amount value:', error);
                }
            });
        } else {
            console.error('Element with ID "amount" not found');
        }

        if (termInput) {
            termInput.addEventListener('input', function () {
                try {
                    updateTermValue(this.value);
                } catch (error) {
                    console.error('Error updating term value:', error);
                }
            });
        } else {
            console.error('Element with ID "term" not found');
        }
    } catch (error) {
        console.error('Error initializing inputs:', error);
    }
});

function updateAmountValue(value) {
    try {
        const amountValueElement = document.getElementById('amount-value');
        amountValueElement.innerText = `$${parseInt(value).toLocaleString()}`;

        const range = document.getElementById('amount');
        const percent = (value - range.min) / (range.max - range.min) * 100;
        amountValueElement.style.left = `calc(${percent}% + (${8 - percent * 0.15}px))`; // Adjust as necessary
    } catch (error) {
        console.error('Error updating amount value element:', error);
    }
}

function updateTermValue(value) {
    try {
        const termValueElement = document.getElementById('term-value');
        termValueElement.innerText = `${value} months`;

        const range = document.getElementById('term');
        const percent = (value - range.min) / (range.max - range.min) * 100;
        termValueElement.style.left = `calc(${percent}% + (${8 - percent * 0.15}px))`; // Adjust as necessary
    } catch (error) {
        console.error('Error updating term value element:', error);
    }
}
