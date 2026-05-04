import pandas as pd
import hashlib
import re

# BİREBİR C# MANTIĞI
def normalize_url(url):
    if pd.isna(url) or str(url).strip() == "": return "no-website"
    clean = str(url).lower().strip()
    if clean.startswith("https://"): clean = clean[8:]
    if clean.startswith("http://"): clean = clean[7:]
    if clean.startswith("www."): clean = clean[4:]
    if clean.endswith("/"): clean = clean[:-1]
    slash_index = clean.find('/')
    if slash_index > 0: clean = clean[:slash_index]
    return clean

def generate_fingerprint(name, website):
    norm_url = normalize_url(website)
    # C#'taki char.IsLetterOrDigit mantığı
    norm_name = "".join([c.lower() for c in str(name) if c.isalnum()])
    raw = f"{norm_url}|{norm_name}"
    return hashlib.md5(raw.encode()).hexdigest()

# Load CSV
csv_path = r'c:\Users\nefise\Desktop\pdfselenium\concatted_enriched.csv'
df = pd.read_csv(csv_path)

# Sadece ID'si olan satırları al (C# öyle yapıyor)
df = df[df['ID'].notna()]

df['fingerprint'] = df.apply(lambda x: generate_fingerprint(x['Name'], x['Website']), axis=1)

unique_fingerprints = df['fingerprint'].nunique()
total_rows = len(df)

# Çakışanları bul
merged = df.groupby('fingerprint').filter(lambda x: x['ID'].nunique() > 1)

print(f"Total Rows with ID: {total_rows}")
print(f"Unique Fingerprints (C# Logic): {unique_fingerprints}")
print(f"Merged Groups: {merged['fingerprint'].nunique()}")

if unique_fingerprints < 1726:
    print(f"\nUpps! Normalizasyon yüzünden {1726 - unique_fingerprints} girişim birleşmiş.")
