import importlib.util
import sys
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
SCRIPT = ROOT / "scripts" / "github_roadmap_sync.py"
SPEC = importlib.util.spec_from_file_location("github_roadmap_sync", SCRIPT)
github_sync = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = github_sync
SPEC.loader.exec_module(github_sync)


class GitHubRoadmapSyncTests(unittest.TestCase):
    def test_parser_preserves_identity_contract_and_completion(self):
        roadmap = """## M0 — Foundation
- [x] `VMX-0001` `P0` Build the foundation.
  - Scope: Keep one source of truth.
  - Acceptance: The mirror is reproducible.
## M0.2 — Qualification
- [ ] `REL-00002` `P1` Qualify the next milestone.
"""
        tickets = github_sync.parse_roadmap(roadmap)
        self.assertEqual(["VMX-0001", "REL-00002"], [item.identifier for item in tickets])
        self.assertTrue(tickets[0].completed)
        self.assertEqual("M0.2", tickets[1].milestone)
        self.assertIn("Scope: Keep one source", tickets[0].body)
        self.assertEqual("kind:rel", tickets[1].kind_label)

    def test_parser_rejects_ticket_outside_known_milestone(self):
        with self.assertRaises(ValueError):
            github_sync.parse_roadmap("## Notes\n- [ ] `VMX-0001` `P1` Invalid placement.")

    def test_option_lookup_is_exact(self):
        field = {"name": "Priority", "options": [{"id": "one", "name": "P1"}]}
        self.assertEqual("one", github_sync.option_id(field, "P1"))
        with self.assertRaises(RuntimeError):
            github_sync.option_id(field, "P0")


if __name__ == "__main__":
    unittest.main()
