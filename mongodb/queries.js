// ============================================================
// Chinese Auction – שאילתות MongoDB
// הכתיבה היא של הסטודנטית – לא AI
// ============================================================
// הפעלה: העתיקי כל שאילתה ל-MongoDB Compass > Open MongoDB Shell
// ============================================================

use("chinese_auction");

// ── שאילתה 1: כל המתנות בקטגוריית אלקטרוניקה ────────────────
// אופרטור: שדה embedded  (category.name)
// Collection: gifts



// ── שאילתה 2: הזמנות מאושרות (IsConfirmed) עם totalAmount > 200 ──
// אופרטורים: $gt, $eq / $and
// Collection: orders



// ── שאילתה 3: מתנות שמחיר הכרטיס שלהן בין 30 ל-70 ─────────────
// אופרטורים: $gte, $lte  / $and
// Collection: gifts



// ── שאילתה 4: משתמשים עם תפקיד Purchaser בלבד ──────────────────
// אופרטור: $eq  (או ערך ישיר)
// Collection: users



// ── שאילתה 5: הזמנות שמכילות את המתנה "iPhone 15 Pro" ──────────
// אופרטור: $elemMatch על מערך orderItems
// Collection: orders



// ── Aggregation: סכום הכנסות לפי userId ────────────────────────
// שלבים: $match → $group → $sort
// Collection: orders
// רמז לשלבים:
//   1. $match: רק הזמנות IsConfirmed
//   2. $group: לפי userId, $sum על totalAmount
//   3. $sort:  לפי הסכום בסדר יורד
