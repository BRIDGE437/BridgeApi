import pandas as pd
import hashlib
import re

def normalize_url(url):
    if pd.isna(url) or str(url).strip() == "": return "no-website"
    url = str(url).lower().strip()
    url = re.sub(r'^https?://', '', url)
    url = re.sub(r'^www\.', '', url)
    url = url.split('/')[0]
    return url

def generate_fingerprint(name, website):
    norm_url = normalize_url(website)
    norm_name = "".join(filter(str.isalnum, str(name).lower()))
    raw = f"{norm_url}|{norm_name}"
    return hashlib.md5(raw.encode()).hexdigest()

# Load the CSV
csv_path = r'c:\Users\nefise\Desktop\pdfselenium\concatted_enriched.csv'
df = pd.read_csv(csv_path)

df['fingerprint'] = df.apply(lambda x: generate_fingerprint(x['Name'], x['Website']), axis=1)

# Count unique IDs vs unique fingerprints
unique_ids = df['ID'].nunique()
unique_fingerprints = df['fingerprint'].nunique()

# Find IDs that share the same fingerprint
fingerprint_groups = df.groupby('fingerprint')['ID'].unique()
merged_groups = fingerprint_groups[fingerprint_groups.apply(len) > 1]

report = []
report.append(f"Unique IDs: {unique_ids}")
report.append(f"Unique Fingerprints: {unique_fingerprints}")
report.append(f"Total Merged Entities: {len(merged_groups)}")
report.append("\nExample of merged IDs (Same business, different IDs):")

for fp, ids in merged_groups.head(10).items():
    names = df[df['ID'].isin(ids)]['Name'].unique()
    websites = df[df['ID'].isin(ids)]['Website'].unique()
    report.append(f"Fingerprint {fp}: IDs {list(ids)} -> Names: {list(names)}, Websites: {list(websites)}")

with open('fingerprint_analysis.txt', 'w', encoding='utf-8') as f:
    f.write('\n'.join(report))

print("Fingerprint analysis generated: fingerprint_analysis.txt")
