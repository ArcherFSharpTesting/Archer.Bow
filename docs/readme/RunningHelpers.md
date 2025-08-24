<!-- (dl
(section-meta
    (title Making Test Running Easier)
)
) -->


The Bow library provides a set of helper functions to simplify running tests with the runner. These helpers make it easier to filter, seed, and execute tests in a variety of ways, and are available by default.

<!-- (dl (# Helper Functions Overview)) -->

Below are the main helpers provided:

- **run**: Runs all tests using the default runner configuration.
- **runWithSeed**: Runs all tests with a custom random seed for deterministic execution order.
- **filterAndRun**: Runs tests after applying a custom filter function.
- **filterAndRunWith**: Runs tests after applying a filter and using a specific seed.
- **filterByCategories**: Returns a filter function that selects tests by a list of category names.
- **filterByCategory**: Returns a filter function for a single category.
- **bow**: Provides a default instance of the Bow runner.

<!-- (dl (# Usage Examples)) -->

Here are some usage examples:

```fsharp
// Run all tests
defaultRunner |> run

// Run all tests with a specific seed
defaultRunner |> runWithSeed (fun () -> 42)

// Run only tests in the "Unit" category
defaultRunner |> filterAndRun (filterByCategory "Unit")

// Run only tests in "Unit" or "Integration" categories with a specific seed
defaultRunner |> filterAndRunWith 123 (filterByCategories ["Unit"; "Integration"])
```

---

For more details, see the `Values.fs` source file or the Bow library documentation.
