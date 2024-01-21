# BetterItemScan

Better Item Scan is a mod for the game Lethal Company, it makes it easier to scan a group of items to see the price of each of them listed, as well as their combined total and calculates which items meet the quoata

Config options for:

- Showing The 'DebugMode' that visually shows the change in width of the scanner 
- Allowing changes to the width of the scanner
- 'ShowShipTotalOnly' allows you to see the total ship amount only on the ship
- 'CalculateForQuota' calculates the scanned items if they meet the quota an mark it with '*', if the scanned items don't meet the quota the items won't be marked
- 'AdjustScreenPositionXaxis' & 'AdjustScreenPositionYaxis' are to help adjust the ui on the screen's position (recommend adjutments for ultrawide users or also downloading the ultrawide mod)
- 'ShowTotalOnShipOnly' allows you to see the scanned amount only on the ship
- 'ItemScanningUICooldown' allows you to adjust how long the UI Stays on screen
- 'FontSize' allows you to adjust the size of the text on screen. However this may have to be done manually in the Bepinex config folder
- 'ItemTextColorHex' & 'ItemTextCalculatorColorHex' allows you to change the colour of the text of the UI for items that meet the quota and items by default

recent changes
- Added config option 'ShowTotalOnShipOnly'
- Added config option 'CalculateForQuota'
- Added config option 'ShowOnShipOnly'
- Added config option 'ItemScanningUICooldown'
- Added config option 'FontSize'
- Added colour config options 'ItemTextColorHex' & 'ItemTextCalculatorColorHex'
- UI adjusted so multiple items are shown as 'amount' x 'scrap' - 'price'
- Adjusted calculator to account for the UI changes. For example if only 2 out 3 items meet the quota for quota calculation, 2 will be shown as meeting the quota and the remaining 1 will be shown as not
- Added config option 'logAllScannedItems' that allows you to output the log of your scanned items into the output log

Small bug fixes
- no longer being able to peak through walls
- no random pink square from DebugMode
- not calculating scrapvalue properlly to meet the quota
- made the calculating of scrap smarter, added braincells to the scanner
- forced the calculator to go to school to become smarter
- the calculator has been tutored
- you can now only scan scrap items not just items with scrap values
- 'total scanned' & 'ship total' now show on a new line and not the same line
- scanning only happened once, it now does so every time
- turning off 'CalculateForQuota' shows items scanned normallly again
- duplicate items weren't being removed due to quota calculation testing
- GiftBoxItem (presents/gifts) are no longer calculated when on the ship if used to get its held item

Bugs and errors? 
-Head to the Lethal Company Discord(s) -> Mod releases -> Better Item Scan
