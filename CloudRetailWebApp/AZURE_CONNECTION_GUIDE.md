# Azure Connection Guide - Step by Step

This guide shows you exactly what to add to connect your MVC app to Azure SQL Database, Azure Storage, and Azure Functions.

---

## 📋 STEP 1: Azure SQL Database Connection

### Where to Get Your SQL Connection String:

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to: **SQL databases** → Select your database (`muzisql2`)
3. Click **Connection strings** in the left menu
4. Copy the **ADO.NET** connection string
5. Replace `{your_password}` with your actual SQL Server password

### What to Update in `appsettings.json`:

**Current (Line 19):**
```json
"SqlConnectionString": "Server=tcp:muzisqldatabase22.database.windows.net,1433;Initial Catalog=muzisql2;Persist Security Info=False;User ID=muzisql22;Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

**Replace `{your_password}` with your actual password:**
```json
"SqlConnectionString": "Server=tcp:muzisqldatabase22.database.windows.net,1433;Initial Catalog=muzisql2;Persist Security Info=False;User ID=muzisql22;Password=YOUR_ACTUAL_PASSWORD_HERE;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

**Example:**
```json
"SqlConnectionString": "Server=tcp:muzisqldatabase22.database.windows.net,1433;Initial Catalog=muzisql2;Persist Security Info=False;User ID=muzisql22;Password=MySecurePass123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

---

## 📋 STEP 2: Azure Storage Account Connection

### Where to Get Your Storage Connection String:

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to: **Storage accounts** → Select your storage account (`muzi22storage`)
3. Click **Access keys** in the left menu
4. Click **Show** next to `key1` or `key2`
5. Copy the **Connection string** (it starts with `DefaultEndpointsProtocol=https;...`)

### What to Update in `appsettings.json`:

**Current (Line 9) - Already configured:**
```json
"StorageConnectionString": "DefaultEndpointsProtocol=https;AccountName=muzi22storage;AccountKey=LmPjwK8ExoCLmd3DnV0RmOcH6pYY7jGwVOYJb3uWM2YCXGA2eTimiTQuj5IZjtSiU5XWN6jH4rD/+AStAfXZaw==;EndpointSuffix=core.windows.net"
```

**✅ This looks correct!** If it's not working, get a fresh connection string from Azure Portal.

**Also verify these storage resources exist:**
- **Blob Container:** `productimages` (Line 10)
- **Table:** `Customers` (Line 11)
- **Table:** `Products` (Line 12)
- **Queue:** `ordersqueue` (Line 13)
- **File Share:** `contracts` (Line 14)

**Note:** The app will automatically create these if they don't exist when it starts.

---

## 📋 STEP 3: Azure Functions App Connection

### Where to Get Your Function App URL:

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to: **Function Apps** → Select your function app (`muzifunction`)
3. Click **Overview** in the left menu
4. Copy the **URL** (it looks like: `https://muzifunction-xxxxx.azurewebsites.net`)

### What to Update in `appsettings.json`:

**Current (Line 15) - Already configured:**
```json
"FunctionBaseUrl": "https://muzifunction-djfheuapedaqgghu.southafricanorth-01.azurewebsites.net/api/"
```

**✅ This looks correct!** Make sure it ends with `/api/`

### Where to Get Your Function Key:

1. In your Function App, click **Functions** in the left menu
2. Select your function (e.g., `TableFunction`, `QueueFunction`, etc.)
3. Click **Function Keys** in the top menu
4. Copy the **default** key value (or create a new one)

### What to Update in `appsettings.json`:

**Current (Line 16):**
```json
"FunctionKey": "YOUR_FUNCTION_KEY"
```

**Replace with your actual function key:**
```json
"FunctionKey": "your-actual-function-key-here"
```

**Example:**
```json
"FunctionKey": "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0"
```

---

## 📋 COMPLETE `appsettings.json` EXAMPLE

Here's what your complete `appsettings.json` should look like:

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
    "FunctionKey": "YOUR_ACTUAL_FUNCTION_KEY_HERE"
  },
  "ConnectionStrings": {
    "SqlConnectionString": "Server=tcp:muzisqldatabase22.database.windows.net,1433;Initial Catalog=muzisql2;Persist Security Info=False;User ID=muzisql22;Password=YOUR_ACTUAL_SQL_PASSWORD_HERE;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
    "StorageConnectionString": "DefaultEndpointsProtocol=https;AccountName=muzi22storage;AccountKey=LmPjwK8ExoCLmd3DnV0RmOcH6pYY7jGwVOYJb3uWM2YCXGA2eTimiTQuj5IZjtSiU5XWN6jH4rD/+AStAfXZaw==;EndpointSuffix=core.windows.net"
  },
  "AllowedHosts": "*"
}
```

---

## ✅ VERIFICATION CHECKLIST

After updating `appsettings.json`, verify:

- [ ] SQL Connection String has real password (not `{your_password}`)
- [ ] Storage Connection String is valid (from Azure Portal)
- [ ] Function Base URL is correct and ends with `/api/`
- [ ] Function Key is set (not `YOUR_FUNCTION_KEY`)

---

## 🚀 TESTING THE CONNECTIONS

1. **Test SQL Database:**
   - Try to register a new user
   - If it works, SQL connection is good!

2. **Test Azure Storage:**
   - Try creating a product (as Admin)
   - Try viewing customers
   - If these work, Storage connection is good!

3. **Test Azure Functions:**
   - Try uploading/downloading contracts
   - Check queue messages in Admin dashboard
   - If these work, Functions connection is good!

---

## 🔧 TROUBLESHOOTING

### SQL Database Connection Fails:
- Check firewall rules in Azure SQL Database
- Go to: SQL Server → Networking → Add your IP address
- Verify password is correct (no special characters need escaping)

### Storage Connection Fails:
- Verify storage account name is correct
- Verify access key is correct
- Check if storage account is in same region

### Functions Connection Fails:
- Verify Function App URL is correct
- Verify Function Key is correct
- Check if Function App is running (not stopped)
- Verify CORS settings if calling from browser

---

## 📝 QUICK REFERENCE

| Service | Location in Azure Portal | What to Copy |
|---------|-------------------------|--------------|
| **SQL Database** | SQL databases → Your DB → Connection strings | ADO.NET connection string |
| **Storage Account** | Storage accounts → Your Account → Access keys | Connection string |
| **Function App URL** | Function Apps → Your App → Overview | URL |
| **Function Key** | Function Apps → Your App → Functions → Function Keys | Default key |

---

**Need Help?** If you're still having issues, check the application logs or browser console for specific error messages.

