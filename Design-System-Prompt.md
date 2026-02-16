# Universal Design System Prompt — SolarEdge-Inspired Style
## Use this prompt to instruct any AI to design your application in this visual style.
## Works for: WPF, WinForms, Qt, Electron, Flutter, SwiftUI, React, Angular, Web, Mobile, Desktop — any UI framework.

---

## HOW TO USE THIS FILE

Copy the entire "PROMPT START" to "PROMPT END" section below and paste it into your conversation with an AI assistant (such as Claude). Prepend or append your own application-specific request, for example:

> "Build me a dashboard app in [your framework] with the following design system: [paste this prompt]"

---

## --- PROMPT START ---

Design my application using the following design system. This is a modern, clean, premium corporate/technology visual language — spacious, professional, and visually striking with bold gradient accents against a clean white canvas. Apply these specifications regardless of the UI framework or platform being used.

---

### 1. COLOR PALETTE

| Token Name           | Hex       | RGB              | Usage                                                                 |
|----------------------|-----------|------------------|-----------------------------------------------------------------------|
| Primary Red          | #EB002C   | rgb(235, 0, 44)  | Gradient start for accents, headings, buttons                         |
| Primary Orange       | #FF6E1E   | rgb(255, 110, 30) | Gradient end for accents, headings, buttons                          |
| Dark Navy            | #001446   | rgb(0, 20, 70)   | Footer background, CTA bar, secondary button fills, dark UI surfaces |
| Accent Red           | #EC012D   | rgb(236, 1, 45)  | Highlighted nav items, alert accents, active states                  |
| Body Text            | #1D1D1F   | rgb(29, 29, 31)  | Primary body text, large headings on light backgrounds               |
| White                | #FFFFFF   | rgb(255, 255, 255)| Backgrounds, text on dark surfaces, card surfaces                    |
| Light Background     | #F5F7FA   | rgb(245, 247, 250)| Subtle section backgrounds, alternating section tint                 |
| Separator Light      | rgba(255,255,255,0.3) | — | Vertical/horizontal dividers on dark or image backgrounds    |

**Primary Gradient (Signature Accent):**
- Direction: Left to right (horizontal, 90°)
- Stops: #EB002C at 1% → #FF6E1E at 67%
- This is the signature visual motif. Use it for: section headings text fill, stat/KPI numbers, primary call-to-action buttons.

**Button Gradient (Variant):**
- Direction: Left to right (horizontal, 90°)
- Stops: #EB002C at 1% → #FF6E1E at 100%
- Used for filled primary action buttons.

---

### 2. TYPOGRAPHY

| Element               | Size (px) | Weight         | Color / Fill                     | Font Family                          |
|-----------------------|-----------|----------------|----------------------------------|--------------------------------------|
| Hero Title            | 50–67     | 600 (SemiBold) | Lines 1–2: Primary Gradient; Line 3: White (on dark bg) | Inter, Segoe UI, SF Pro, Roboto, sans-serif |
| Section Heading (H2)  | 50        | 700 (Bold)     | Primary Gradient (text fill)     | Inter, Segoe UI, SF Pro, Roboto, sans-serif |
| Sub-heading / Card Title | 18–22  | 600 (SemiBold) | Dark Navy #001446 or Body Text #1D1D1F | Inter, Segoe UI, SF Pro, Roboto, sans-serif |
| Body / Paragraph      | 14–16     | 400 (Regular)  | Body Text #1D1D1F                | Inter, Segoe UI, SF Pro, Roboto, sans-serif |
| Navigation Links      | 16        | 400 (Regular)  | Black #000000                    | Inter, Segoe UI, SF Pro, Roboto, sans-serif |
| Stat / KPI Numbers    | 50        | 700 (Bold)     | Primary Gradient (text fill)     | Inter, Segoe UI, SF Pro, Roboto, sans-serif |
| Stat Labels           | 14–16     | 400–500        | White #FFFFFF (on dark bg)       | Inter, Segoe UI, SF Pro, Roboto, sans-serif |
| Footer Links          | 14        | 400 (Regular)  | White #FFFFFF                    | Inter, Segoe UI, SF Pro, Roboto, sans-serif |
| Footer Heading        | 16        | 600 (SemiBold) | White #FFFFFF                    | Inter, Segoe UI, SF Pro, Roboto, sans-serif |

**Primary font stack (in order of preference):** Inter → Segoe UI → SF Pro Display → Roboto → Arial → Helvetica → sans-serif.
Use whichever is available on the target platform.

**Line height:** 1.5× for body text. 1.2× for headings.

---

### 3. SPACING & LAYOUT PRINCIPLES

- **Section vertical padding:** 60–100px top and bottom.
- **Content max width:** ~1200px, horizontally centered.
- **Full-bleed backgrounds:** Images and colored backgrounds extend to the full width of the viewport/window, while text content stays within the max-width container.
- **Card grid gap:** 20–30px between cards.
- **Nav bar height:** ~55–60px.
- **General padding inside cards:** 20–24px.
- **Generous whitespace** is key — do not crowd elements. Let the design breathe.

---

### 4. COMPONENT SPECIFICATIONS

#### 4.1 Navigation Bar
- **Background:** White (#FFFFFF)
- **Height:** 55–60px
- **Position:** Fixed/sticky at top of window
- **Layout:** Logo on the left, navigation links center-right, search icon far right
- **Logo:** Placeholder with a red accent element (matching #EB002C)
- **Nav links:** 16px, regular weight, black. ~40px spacing between items.
- **Special/highlighted link:** Uses Accent Red (#EC012D) color.
- **Dropdown indicator:** Small chevron (▾) after items with sub-menus.
- **Search icon:** Magnifying glass icon, dark grey or black.
- **Shadow:** None or very subtle (0 1px 3px rgba(0,0,0,0.05)) to separate from content.

#### 4.2 Hero / Banner Section
- **Height:** 500–800px (or responsive to viewport)
- **Background:** Full-width high-quality image (cityscape, technology, or relevant to your domain). Apply a subtle dark overlay if needed for text legibility.
- **Heading:** Large (50–67px), SemiBold. Multi-line layout:
  - Lines 1 and 2: Gradient-filled text (Primary Gradient: #EB002C → #FF6E1E)
  - Line 3: White text (#FFFFFF)
- **Text alignment:** Left-aligned, vertically centered, positioned in the left 40–60% of the section.

#### 4.3 Feature / Pillar Cards Row (e.g. "Our Core Power")
- **Section title:** Centered, gradient text.
- **Layout:** 3 equal-width cards in a horizontal row, edge-to-edge (no gap between them).
- **Each card:** Background image, with a bold white heading at the bottom-left (20–30px padding). The middle card may include a paragraph of white descriptive text.
- **Text shadow:** Subtle shadow on white text for legibility over images (0 1px 4px rgba(0,0,0,0.5)).
- **Card border-radius:** 0 (flush, edge-to-edge).

#### 4.4 Stats / KPI Banner
- **Background:** Full-width image with semi-transparent overlay, or a subtle dark background.
- **Layout:** 4 stat items in an evenly spaced horizontal row.
- **Each stat:**
  - Number: 50px, Bold, gradient-filled text (Primary Gradient).
  - Label: 14–16px, white, centered below the number.
- **Dividers:** 1px vertical lines between stat items, white at ~30% opacity.

#### 4.5 Section Heading (Reusable)
- **Font size:** 50px
- **Font weight:** 700 (Bold)
- **Text fill:** Primary Gradient (#EB002C → #FF6E1E, horizontal)
- **Alignment:** Left-aligned (matching the content grid start)
- **Margin bottom:** 30–50px below the heading before content

#### 4.6 Content Cards (News / Blog / Items)
- **Background:** White (#FFFFFF)
- **Border radius:** 8px
- **Shadow:** 0 2px 8px rgba(0,0,0,0.08)
- **Border:** None
- **Layout (vertical):**
  1. Top image (fills card width, clips to top border-radius)
  2. Title: 18–20px, SemiBold, Dark Navy or Body Text color
  3. Date/metadata: 14px, regular, grey (#666666)
  4. Body text: 14–16px, regular, Body Text color
  5. Action link at bottom: "Discover more >" in Dark Navy (#001446), with a right arrow (›)
- **Card internal padding:** 20–24px (sides and bottom). No padding on the top image.

#### 4.7 Primary CTA Button (Gradient)
- **Background:** Button Gradient (linear, #EB002C → #FF6E1E, left-to-right)
- **Text color:** White (#FFFFFF)
- **Font size:** 16px
- **Font weight:** 700 (Bold)
- **Padding:** 10px vertical, 30px horizontal
- **Border radius:** 20px (pill / capsule shape)
- **Border:** None
- **Hover state:** Slightly darken the gradient (reduce brightness by ~10%) or add a subtle inner shadow
- **Active state:** Scale down to 0.97 or darken further

#### 4.8 Secondary / Outlined Button
- **Background:** Transparent
- **Text color:** White (#FFFFFF) — when on dark backgrounds; Dark Navy (#001446) on light backgrounds
- **Border:** 1.5px solid, same color as text
- **Border radius:** 20px (pill / capsule)
- **Padding:** 10px vertical, 30px horizontal
- **Hover state:** Fill with white background, text becomes Dark Navy (#001446)

#### 4.9 Two-Column Feature Section (e.g. "Support and Troubleshooting")
- **Layout:** Two columns, roughly 45% / 55% split.
- **Left column:** Large image, border-radius 0 or 8px.
- **Right column:** Large heading (30–36px, Bold, Body Text color), paragraph text (16px), and a Primary CTA Button below.
- **Vertical alignment:** Content centered vertically within the row.

#### 4.10 Footer CTA Bar
- **Background:** Dark Navy (#001446)
- **Layout:** Full width. Left side: heading ("How can we help you?") in white, subtitle below in lighter white. Center: an outlined or filled button (e.g., "Contact Us"). Right side: social media icons in white.
- **Padding:** 30–40px vertical.

#### 4.11 Footer
- **Background:** Dark Navy (#001446), same as the CTA bar (seamless continuation).
- **Layout:** 3–4 columns of links.
  - Column headings: 16px, SemiBold, white.
  - Links: 14px, regular, white. Hover: underline.
- **Bottom bar:** A thin horizontal line (1px, rgba(255,255,255,0.2)), then copyright text + policy links in 12–14px white, separated by "/" characters.

---

### 5. INTERACTION & ANIMATION GUIDELINES

- **Hover on nav links:** Color shifts to Accent Red (#EC012D) or a subtle underline appears.
- **Hover on cards:** Slight upward translate (translateY -4px) and shadow deepens.
- **Hover on CTA buttons:** Gradient darkens slightly, optional subtle scale-up (1.02).
- **Page transitions / scroll:** Sections fade in and slide up slightly as they enter the viewport (optional but adds polish).
- **Transition duration:** 200–300ms ease-in-out for all hover/interactive states.

---

### 6. ICONOGRAPHY

- **Style:** Line icons (not filled), 1.5–2px stroke weight. Clean and minimal.
- **Color:** Match the surrounding text color (black on light, white on dark).
- **Size:** 20–24px for nav icons, 24–32px for social media icons in the footer.
- **Social icons used:** Instagram, LinkedIn, Facebook, X (Twitter), YouTube.
- **Recommended icon set:** Lucide, Feather, Phosphor, or platform-native equivalents.

---

### 7. RESPONSIVE / ADAPTIVE BEHAVIOR

- **Large screens (>1400px):** Content centered at 1200px max-width. Full-bleed backgrounds.
- **Medium screens (768–1400px):** Content adapts, card rows may reduce from 3 to 2 columns. Nav collapses to hamburger menu.
- **Small screens (<768px):** Single column layout. Hero text shrinks to 32–40px. Cards stack vertically. Footer columns stack. Stats wrap to 2×2 grid.
- **Nav on mobile:** Hamburger icon → full-screen or slide-in menu panel. White background, dark text.

---

### 8. DESIGN PRINCIPLES SUMMARY

1. **Generous whitespace** — Let the design breathe. Avoid clutter.
2. **Gradient as signature** — The red→orange gradient is the single most recognizable visual motif. Use it consistently but sparingly: headings, key numbers, and primary buttons only.
3. **Dark navy for authority** — The footer and CTA areas use #001446 to convey stability and trust.
4. **Photography-driven** — Large, high-quality images as section backgrounds. Text overlaid with careful contrast.
5. **Minimal borders** — Rely on whitespace, shadow, and background color changes to separate elements, not borders.
6. **Professional yet energetic** — The warmth of the red-orange gradient balances the corporate seriousness of the navy and clean typography.
7. **Consistency** — Every heading uses the same gradient. Every primary button uses the same pill shape and gradient fill. Every card follows the same shadow and radius.

---

## --- PROMPT END ---
