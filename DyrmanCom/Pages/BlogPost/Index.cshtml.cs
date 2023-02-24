using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DyrmanCom.Pages.BlogPost;

public class Index : PageModel
{
    public int Id { get; private set; }

    public IActionResult OnGet(int id)
    {
        if (id == 0)
        {
            return NotFound();
        }

        Id = id;
        return Page();
    }
}
