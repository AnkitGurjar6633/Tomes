document.addEventListener("DOMContentLoaded", function () {

    const userReviewSection = document.getElementById("userReviewSection");
    const editReviewButton = document.getElementById("editReviewButton");
    const editReviewForm = document.getElementById("editReviewForm");
    const cancelEditButton = document.getElementById("cancelEdit");

    function handleStarRating(containerSelector, hiddenInputId) {
        const stars = document.querySelectorAll(`${containerSelector} .star-ratings-input .star`);
        const ratingInput = document.getElementById(hiddenInputId);

        stars.forEach(star => {
            star.addEventListener('click', () => {
                const ratingValue = parseInt(star.getAttribute('data-rating'))
                ratingInput.value = ratingValue;

                stars.forEach(star => {
                    if (parseInt(star.getAttribute('data-rating')) <= ratingValue) {
                        star.classList.add('active-rating')
                        star.innerHTML = '<i class="bi bi-star-fill"></i>'
                    }
                    else {
                        star.classList.remove('active-rating')
                        star.innerHTML = '<i class="bi bi-star"></i>';
                    }
                })
            })
        })
    }

    handleStarRating("", 'hidden-rating');
    handleStarRating("#editReviewForm", 'hidden-rating-edit');

    editReviewButton.addEventListener("click", () => {
        userReviewSection.classList.add("d-none");
        editReviewForm.classList.remove("d-none");
    });

    cancelEditButton.addEventListener("click", () => {
        userReviewSection.classList.remove("d-none");
        editReviewForm.classList.add("d-none");

    });
});