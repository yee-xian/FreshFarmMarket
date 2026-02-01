using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Model;
using WebApplication1.Services;

namespace WebApplication1.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<IndexModel> _logger;

        public ApplicationUser? CurrentUser { get; set; }
        public string? DecryptedCreditCard { get; set; }

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            IEncryptionService encryptionService,
            ILogger<IndexModel> logger)
        {
            _userManager = userManager;
            _encryptionService = encryptionService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            CurrentUser = await _userManager.GetUserAsync(User);

            if (CurrentUser == null)
            {
                return RedirectToPage("/Login");
            }

            // Decrypt credit card for display
            try
            {
                DecryptedCreditCard = _encryptionService.Decrypt(CurrentUser.EncryptedCreditCard);
                // Mask for display - show only last 4 digits
                // DecryptedCreditCard = MaskCreditCard(DecryptedCreditCard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting credit card for user {UserId}", CurrentUser.Id);
                DecryptedCreditCard = "Error decrypting";
            }

            return Page();
        }

        private string MaskCreditCard(string creditCard)
        {
            if (string.IsNullOrEmpty(creditCard) || creditCard.Length < 4)
                return "****";

            return new string('*', creditCard.Length - 4) + creditCard[^4..];
        }
    }
}
