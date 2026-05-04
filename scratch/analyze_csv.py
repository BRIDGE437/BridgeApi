import pandas as pd

# Load the CSV
csv_path = r'c:\Users\nefise\Desktop\pdfselenium\concatted_enriched.csv'
df = pd.read_csv(csv_path)

# 1. Total records
total_rows = len(df)

# 2. Unique IDs
unique_ids = df['ID'].nunique()

# 3. Analyze Duplicate IDs
duplicates = df[df.duplicated('ID', keep=False)].sort_values('ID')

report = []
report.append(f"--- CSV DATA INTEGRITY REPORT ---")
report.append(f"Total Rows: {total_rows}")
report.append(f"Unique IDs: {unique_ids}")
report.append(f"Total Duplicate ID Rows: {len(duplicates)}")

# Check if same ID rows have different data
diff_reports = []
for startup_id, group in duplicates.groupby('ID'):
    names = group['Name'].nunique()
    websites = group['Website'].nunique()
    emails = group['Website_Email'].nunique()
    
    if names > 1 or websites > 1 or emails > 1:
        diff_reports.append({
            'ID': startup_id,
            'Name_Count': names,
            'Website_Count': websites,
            'Email_Count': emails,
            'Names': group['Name'].unique().tolist(),
            'Websites': group['Website'].unique().tolist()
        })

report.append(f"\n--- CONFLICT ANALYSIS (Same ID but Different Data) ---")
report.append(f"Total Conflicting IDs: {len(diff_reports)}")

if diff_reports:
    report.append("\nTop 5 Conflicts Example:")
    for conflict in diff_reports[:5]:
        report.append(f"ID {conflict['ID']}: Names={conflict['Names']}, Websites={conflict['Websites']}")

# Output to a file
with open('csv_integrity_report.txt', 'w', encoding='utf-8') as f:
    f.write('\n'.join(report))

print("Report generated: csv_integrity_report.txt")
