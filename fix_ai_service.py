import os
import re

directory = r'c:\Users\nefise\Desktop\BridgeApi\ai-service'

replacements = {
    '\"Startups\"': '\"StartupProfiles\"',
    '\"Id\"': '\"UserId\"',
    'startup_id: int': 'startup_id: str',
    'startup_ids: list[int]': 'startup_ids: list[str]',
    'source_id: int': 'source_id: str',
    'target_ids: list[int]': 'target_ids: list[str]',
    'source_startup_id: int': 'source_startup_id: str',
    'target_startup_ids: list[int]': 'target_startup_ids: list[str]',
    'id: int': 'id: str',
    'id: {': 'id: {', # Keep this
}

for root, dirs, files in os.walk(directory):
    for file in files:
        if file.endswith('.py'):
            path = os.path.join(root, file)
            with open(path, 'r', encoding='utf-8') as f:
                content = f.read()
            
            original = content
            for old, new in replacements.items():
                content = content.replace(old, new)
            
            if content != original:
                with open(path, 'w', encoding='utf-8') as f:
                    f.write(content)
                print(f"Updated {path}")
