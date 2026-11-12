# Testing Notes

## 1. Test Summary
* **Total Tests:** 16
* **Passing:** 16
* **Failing:** 0

## 2. Line Coverage
* **Overall Line Coverage:** 90.5%
* **Domain Layer:** 100%
* **Application Layer:** 95.8%

## 3. Tooling Usage
* **Moq:** Used to mock application interfaces (`IOrderRepository`, `IInventoryService`, `INotificationService`) and verify method call counts.
* **AutoFixture:** Used to generate fully populated `Order` test objects with customized unit prices and quantities.
* **Bogus:** Used to generate deterministic fake customer names and emails using a fixed seed.

## 4. Discount Threshold Anomaly Analysis
No, it creates a discount threshold anomaly where spending 0.01 EGP more drops the final total by 99.99 EGP.

## 5. Identified Bugs
In `CalculateDiscount`, the formula uses fixed string-like logic for subtotal discount, leading to edge-case anomalies around the threshold (e.g., 999.99 EGP total vs 1,000 EGP total).