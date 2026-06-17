// ============================================================
// Chinese Auction System – MongoDB Seed Script
// המרה ממסד נתונים רלציוני (Entity Framework / SQL Server)
// ============================================================
//
// מבנה רלציוני מקורי:
//   Users, Categories, Donors, Gifts (FK→Category, FK→Donor),
//   Orders (FK→User), OrderItems (FK→Order, FK→Gift), Winners (FK→Gift, FK→User)
//
// החלטות עיצוב (MongoDB):
//   1. OrderItems הייתה טבלת צומת (junction table) לפתרון Many-to-Many.
//      ב-MongoDB משלבים אותה כ-array embedded בתוך orders – אין צורך בטבלת ביניים.
//   2. Category ו-Donor מוטמעים כ-snapshot בתוך כל Gift (embedding),
//      כי הם מוצגים תמיד יחד עם המתנה ולא משתנים תדיר.
//   3. Winners נשמרת כקולקציה נפרדת עם שמות משוכפלים (denormalization)
//      לנוחות שליפה.
//   4. מזהי Id נשמרו כמספרים שלמים (1,2,3...) כדי לשמור על
//      עקביות עם ה-FK-ים המקוריים.
// ============================================================

use("chinese_auction");

// ── ניקוי ────────────────────────────────────────────────────
db.users.drop();
db.categories.drop();
db.donors.drop();
db.gifts.drop();
db.orders.drop();
db.winners.drop();

// ── 1. Categories ─────────────────────────────────────────────
db.categories.insertMany([
  { _id: 1, name: "אלקטרוניקה" },
  { _id: 2, name: "ספא ובריאות" },
  { _id: 3, name: "ספורט ופנאי" },
  { _id: 4, name: "מטבח" },
  { _id: 5, name: "נסיעות" }
]);

// ── 2. Donors ─────────────────────────────────────────────────
db.donors.insertMany([
  { _id: 1, name: "אלקטרו שלמה בע\"מ",   email: "shlomo@electro.co.il",       phone: "050-1111111" },
  { _id: 2, name: "מלון ים המלח",          email: "hotel@deadsea.co.il",        phone: "050-2222222" },
  { _id: 3, name: "ספורטק",               email: "info@sportek.co.il",          phone: "050-3333333" },
  { _id: 4, name: "רשת סופר-פארם",        email: "gifts@super-pharm.co.il",    phone: "050-4444444" },
  { _id: 5, name: "חברת תיירות שמש",      email: "travel@shemesh.co.il",       phone: "050-5555555" }
]);

// ── 3. Gifts  (Category + Donor embedded) ─────────────────────
db.gifts.insertMany([
  {
    _id: 1,
    name: "iPhone 15 Pro",
    description: "סמארטפון מתקדם של אפל עם מצלמה 48MP",
    ticketPrice: 50,
    imageUrl: "https://example.com/iphone15.jpg",
    category: { _id: 1, name: "אלקטרוניקה" },
    donor:    { _id: 1, name: "אלקטרו שלמה בע\"מ", email: "shlomo@electro.co.il", phone: "050-1111111" }
  },
  {
    _id: 2,
    name: "לפטופ Dell XPS 13",
    description: "מחשב נייד חזק לעסקים ויוצרי תוכן",
    ticketPrice: 75,
    imageUrl: "https://example.com/dell.jpg",
    category: { _id: 1, name: "אלקטרוניקה" },
    donor:    { _id: 1, name: "אלקטרו שלמה בע\"מ", email: "shlomo@electro.co.il", phone: "050-1111111" }
  },
  {
    _id: 3,
    name: "סוף שבוע ספא ים המלח",
    description: "לינה זוגית + טיפולי ספא בים המלח",
    ticketPrice: 30,
    imageUrl: null,
    category: { _id: 2, name: "ספא ובריאות" },
    donor:    { _id: 2, name: "מלון ים המלח", email: "hotel@deadsea.co.il", phone: "050-2222222" }
  },
  {
    _id: 4,
    name: "אופניים חשמליים",
    description: "אופניים חשמליים 250W עם סוללה 36V",
    ticketPrice: 40,
    imageUrl: "https://example.com/ebike.jpg",
    category: { _id: 3, name: "ספורט ופנאי" },
    donor:    { _id: 3, name: "ספורטק", email: "info@sportek.co.il", phone: "050-3333333" }
  },
  {
    _id: 5,
    name: "קורס בישול גורמה",
    description: "6 מפגשים עם שף מקצועי",
    ticketPrice: 20,
    imageUrl: null,
    category: { _id: 4, name: "מטבח" },
    donor:    { _id: 4, name: "רשת סופר-פארם", email: "gifts@super-pharm.co.il", phone: "050-4444444" }
  },
  {
    _id: 6,
    name: "טיסה לברצלונה זוגי",
    description: "כרטיסי טיסה הלוך ושוב לברצלונה",
    ticketPrice: 100,
    imageUrl: "https://example.com/barcelona.jpg",
    category: { _id: 5, name: "נסיעות" },
    donor:    { _id: 5, name: "חברת תיירות שמש", email: "travel@shemesh.co.il", phone: "050-5555555" }
  },
  {
    _id: 7,
    name: "Samsung TV 65 אינץ'",
    description: "טלוויזיה חכמה QLED 4K",
    ticketPrice: 60,
    imageUrl: "https://example.com/samsung.jpg",
    category: { _id: 1, name: "אלקטרוניקה" },
    donor:    { _id: 1, name: "אלקטרו שלמה בע\"מ", email: "shlomo@electro.co.il", phone: "050-1111111" }
  },
  {
    _id: 8,
    name: "ערכת כושר ביתית",
    description: "משקולות, גומיות ומזרן יוגה",
    ticketPrice: 25,
    imageUrl: null,
    category: { _id: 3, name: "ספורט ופנאי" },
    donor:    { _id: 3, name: "ספורטק", email: "info@sportek.co.il", phone: "050-3333333" }
  }
]);

// ── 4. Users ──────────────────────────────────────────────────
db.users.insertMany([
  { _id: 1, name: "ישראל ישראלי", email: "israel@example.com", passwordHash: "$2a$11$abc111", phone: "052-1234567", role: "Admin"     },
  { _id: 2, name: "שרה כהן",      email: "sara@example.com",   passwordHash: "$2a$11$abc222", phone: "052-2345678", role: "Purchaser" },
  { _id: 3, name: "דוד לוי",      email: "david@example.com",  passwordHash: "$2a$11$abc333", phone: "052-3456789", role: "Purchaser" },
  { _id: 4, name: "רחל מזרחי",    email: "rachel@example.com", passwordHash: "$2a$11$abc444", phone: "052-4567890", role: "Purchaser" },
  { _id: 5, name: "יוסף אברהם",   email: "yosef@example.com",  passwordHash: "$2a$11$abc555", phone: "052-5678901", role: "Purchaser" }
]);

// ── 5. Orders  (OrderItems embedded – לשעבר טבלת צומת) ───────
db.orders.insertMany([
  {
    _id: 1,
    orderDate:   ISODate("2025-12-15T10:30:00Z"),
    status:      "IsConfirmed",
    totalAmount: 310,
    userId:      2,
    orderItems: [
      { _id: 1, giftId: 1, giftName: "iPhone 15 Pro",        quantity: 5, ticketPrice: 50 },
      { _id: 2, giftId: 7, giftName: "Samsung TV 65 אינץ'",  quantity: 1, ticketPrice: 60 }
    ]
  },
  {
    _id: 2,
    orderDate:   ISODate("2025-12-16T14:00:00Z"),
    status:      "IsConfirmed",
    totalAmount: 300,
    userId:      3,
    orderItems: [
      { _id: 3, giftId: 6, giftName: "טיסה לברצלונה זוגי", quantity: 3, ticketPrice: 100 }
    ]
  },
  {
    _id: 3,
    orderDate:   ISODate("2025-12-17T09:15:00Z"),
    status:      "IsDraft",
    totalAmount: 95,
    userId:      4,
    orderItems: [
      { _id: 4, giftId: 3, giftName: "סוף שבוע ספא ים המלח", quantity: 2, ticketPrice: 30 },
      { _id: 5, giftId: 5, giftName: "קורס בישול גורמה",     quantity: 1, ticketPrice: 20 },
      { _id: 6, giftId: 8, giftName: "ערכת כושר ביתית",      quantity: 1, ticketPrice: 25 }
    ]
  },
  {
    _id: 4,
    orderDate:   ISODate("2025-12-18T11:45:00Z"),
    status:      "IsConfirmed",
    totalAmount: 200,
    userId:      2,
    orderItems: [
      { _id: 7, giftId: 4, giftName: "אופניים חשמליים", quantity: 5, ticketPrice: 40 }
    ]
  },
  {
    _id: 5,
    orderDate:   ISODate("2025-12-19T16:00:00Z"),
    status:      "IsConfirmed",
    totalAmount: 375,
    userId:      5,
    orderItems: [
      { _id: 8, giftId: 2, giftName: "לפטופ Dell XPS 13", quantity: 5, ticketPrice: 75 }
    ]
  },
  {
    _id: 6,
    orderDate:   ISODate("2025-12-20T10:00:00Z"),
    status:      "IsDraft",
    totalAmount: 160,
    userId:      3,
    orderItems: [
      { _id: 9,  giftId: 1, giftName: "iPhone 15 Pro",    quantity: 2, ticketPrice: 50 },
      { _id: 10, giftId: 5, giftName: "קורס בישול גורמה", quantity: 3, ticketPrice: 20 }
    ]
  }
]);

// ── 6. Winners ────────────────────────────────────────────────
db.winners.insertMany([
  { _id: 1, giftId: 1, giftName: "iPhone 15 Pro",       userId: 2, userName: "שרה כהן"  },
  { _id: 2, giftId: 6, giftName: "טיסה לברצלונה זוגי", userId: 3, userName: "דוד לוי"  },
  { _id: 3, giftId: 4, giftName: "אופניים חשמליים",     userId: 2, userName: "שרה כהן"  }
]);

print("=== Seed complete ===");
print("Collections created: users, categories, donors, gifts, orders, winners");
