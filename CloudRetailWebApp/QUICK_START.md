# 🚀 Quick Start - Connect Azure Services

## ⚠️ YOU NEED TO UPDATE 2 VALUES IN `appsettings.json`

---

## 1️⃣ SQL Database Password (REQUIRED)

**File:** `appsettings.json`  
**Line:** 19  
**Find:** `Password={your_password};`  
**Replace with:** `Password=YOUR_ACTUAL_PASSWORD;`

### How to get your SQL password:
- This is the password you set when creating the SQL Server
- If you forgot it, you can reset it in Azure Portal:
  - Go to: **SQL servers** → `muzisqldatabase22` → **Reset password**

### Example:
```json
"Password=MySecurePassword123!;"
```

---

## 2️⃣ Azure Functions Key (REQUIRED)

**File:** `appsettings.json`  
**Line:** 16  
**Find:** `"FunctionKey": "YOUR_FUNCTION_KEY"`  
**Replace with:** `"FunctionKey": "your-actual-key-here"`

### How to get your Function Key:

**Option A: From Azure Portal**
1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to: **Function Apps** → `muzifunction`
3. Click **Functions** in left menu
4. Select any function (e.g., `TableFunction`)
5. Click **Function Keys** at the top
6. Copy the **default** key value

**Option B: From Function App Code**
- Check your Function App's `local.settings.json` or Azure Portal Configuration

### Example:
```json
"FunctionKey": "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6"
```

---

## ✅ WHAT'S ALREADY CONFIGURED (No changes needed)

✅ **Azure Storage Connection String** - Already set (Line 9)  
✅ **Function Base URL** - Already set (Line 15)  
✅ **Storage Resources** - Blob, Tables, Queue, File Share names are set

---

## 📝 COMPLETE UPDATED `appsettings.json`

After updates, your file should look like this:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Azure": {
    "StorageConnectionString": "DefaultEndpointsProtocol=https;AccountName=muzi22storage;AccountKey=LmPjwK8ExoCLmd3DnV0RmOcH6pYY7jGwVOYJb3uWM2YCXGA2eTimiTQuj5IZjtSiU5XWN6jH4rD/+AStAfXZaw==;EndpointSuffix=core.windows.net",
    "BlobContainer": "productimages",
    "TableCustomers": "Customers",
    "TableProducts": "Products",
    "QueueOrders": "ordersqueue",
    "FileShareName": "contracts",
    "FunctionBaseUrl": "https://muzifunction-djfheuapedaqgghu.southafricanorth-01.azurewebsites.net/api/",
    "FunctionKey": "PASTE_YOUR_FUNCTION_KEY_HERE"
  },
  "ConnectionStrings": {
    "SqlConnectionString": "Server=tcp:muzisqldatabase22.database.windows.net,1433;Initial Catalog=muzisql2;Persist Security Info=False;User ID=muzisql22;Password=PASTE_YOUR_SQL_PASSWORD_HERE;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
    "StorageConnectionString": "DefaultEndpointsProtocol=https;AccountName=muzi22storage;AccountKey=LmPjwK8ExoCLmd3DnV0RmOcH6pYY7jGwVOYJb3uWM2YCXGA2eTimiTQuj5IZjtSiU5XWN6jH4rD/+AStAfXZaw==;EndpointSuffix=core.windows.net"
  },
  "AllowedHosts": "*"
}
```

---

## 🔍 VERIFICATION

After updating, verify:

1. **SQL Connection:**
   - Open `appsettings.json`
   - Line 19 should NOT contain `{your_password}`
   - Should have your actual password

2. **Function Key:**
   - Line 16 should NOT contain `YOUR_FUNCTION_KEY`
   - Should have your actual function key

---

## 🎯 NEXT STEPS

1. ✅ Update SQL password in `appsettings.json` (Line 19)
2. ✅ Update Function Key in `appsettings.json` (Line 16)
3. ✅ Save the file
4. ✅ Run the application
5. ✅ Try registering a new user (tests SQL)
6. ✅ Try creating a product (tests Storage)
7. ✅ Try uploading a contract (tests Functions)

---

## 🆘 STILL NOT WORKING?

Check the detailed guide: `AZURE_CONNECTION_GUIDE.md`

