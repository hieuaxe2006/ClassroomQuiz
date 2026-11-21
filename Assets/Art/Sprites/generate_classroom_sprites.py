import os
import math
import random
from PIL import Image, ImageDraw, ImageFilter, ImageEnhance, ImageOps

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
PROJECT_ROOT = os.path.abspath(os.path.join(BASE_DIR, "..", "..", ".."))

SRC_IMAGE_PATH = os.path.join(PROJECT_ROOT, "Assets", "Art", "UI", "Gemini_Generated_Image_r1ikpjr1ikpjr1ik.png")
RESOURCES_DIR = os.path.join(PROJECT_ROOT, "Assets", "Resources", "ClassroomQuiz", "Sprites")
CLASSROOM_UI_DIR = os.path.join(PROJECT_ROOT, "Assets", "Art", "Sprites", "ClassroomUI")
BACKGROUNDS_DIR = os.path.join(PROJECT_ROOT, "Assets", "Art", "Sprites", "Backgrounds")

os.makedirs(RESOURCES_DIR, exist_ok=True)
os.makedirs(CLASSROOM_UI_DIR, exist_ok=True)
os.makedirs(BACKGROUNDS_DIR, exist_ok=True)

print("Source image:", SRC_IMAGE_PATH)
src_img = Image.open(SRC_IMAGE_PATH).convert("RGBA")
SW, SH = src_img.size

# Color palette derived from the image
COLOR_WALL_TOP = (112, 178, 203, 255)       # Light blue wall
COLOR_WALL_MID = (142, 201, 218, 255)       # Soft cyan wall
COLOR_FLOOR_ORANGE = (248, 166, 108, 255)   # Peach orange tile
COLOR_FLOOR_YELLOW = (254, 218, 150, 255)   # Warm sunny tile
COLOR_WOOD_DARK = (106, 56, 20, 255)        # Wood shadow / outline
COLOR_WOOD_MID = (142, 90, 43, 255)         # Warm desk / frame wood
COLOR_WOOD_LIGHT = (194, 137, 78, 255)      # Wood highlight
COLOR_GOLD = (241, 196, 15, 255)            # Badge gold / star
COLOR_GOLD_DARK = (198, 149, 10, 255)       # Gold shading
COLOR_BOARD_GREEN = (31, 74, 52, 255)       # Blackboard deep green
COLOR_BOARD_DARK = (21, 52, 36, 255)        # Blackboard dark
COLOR_CHALK_WHITE = (248, 249, 250, 255)    # Clean chalk white
COLOR_RED_FLAG = (231, 76, 60, 255)         # Bunting red
COLOR_YELLOW_FLAG = (243, 156, 18, 255)     # Bunting yellow
COLOR_BLUE_FLAG = (52, 152, 219, 255)       # Bunting blue
COLOR_GREEN_FLAG = (46, 204, 113, 255)      # Bunting green

def save_both(img, name, border=None):
    """Saves to Resources and ClassroomUI, and creates/updates meta if needed"""
    p1 = os.path.join(RESOURCES_DIR, name + ".png")
    p2 = os.path.join(CLASSROOM_UI_DIR, name + ".png")
    p3 = os.path.join(BACKGROUNDS_DIR, name + ".png")
    
    img.save(p1, "PNG")
    img.save(p2, "PNG")
    if "Background" in name or "Map" in name:
        img.save(p3, "PNG")
    print(f"Saved: {name}.png ({img.width}x{img.height})")

def create_16_9_background(src, width=1920, height=1080, crop_mode="fit_height", darken=False, blur=0):
    """Generates a seamless 16:9 background from the classroom source image"""
    sw, sh = src.size
    target_ratio = width / height
    src_ratio = sw / sh
    
    # Scale so that src fills the canvas appropriately
    scale = max(width / sw, height / sh)
    nw, nh = int(sw * scale), int(sh * scale)
    scaled = src.resize((nw, nh), Image.Resampling.LANCZOS)
    
    # Center crop / fit
    left = (nw - width) // 2
    top = (nh - height) // 2
    cropped = scaled.crop((left, top, left + width, top + height))
    
    if blur > 0:
        cropped = cropped.filter(ImageFilter.GaussianBlur(blur))
        
    if darken:
        enhancer = ImageEnhance.Brightness(cropped)
        cropped = enhancer.enhance(0.85)
        
    return cropped

print("Creating backgrounds...")

# 1. BackgroundLobby / StartScene (Full 1920x1080 vibrant classroom)
bg_lobby = create_16_9_background(src_img, 1920, 1080)
save_both(bg_lobby, "BackgroundLobby")
save_both(bg_lobby, "BackgroundStartScene")

# 2. BackgroundLogin (1920x1080 with soft atmospheric warmth for the login card)
bg_login = create_16_9_background(src_img, 1920, 1080, blur=3, darken=True)
# Add subtle vignette to login background
vignette = Image.new("RGBA", (1920, 1080), (0, 0, 0, 0))
vdraw = ImageDraw.Draw(vignette)
cx, cy = 960, 540
max_r = math.sqrt(cx**2 + cy**2)
for y in range(0, 1080, 20):
    for x in range(0, 1920, 20):
        dist = math.sqrt((x - cx)**2 + (y - cy)**2)
        alpha = int(min(120, max(0, (dist - 400) / (max_r - 400) * 120)))
        vdraw.rectangle([x, y, x + 20, y + 20], fill=(15, 30, 45, alpha))
vignette = vignette.filter(ImageFilter.GaussianBlur(25))
bg_login = Image.alpha_composite(bg_login, vignette)
save_both(bg_login, "BackgroundLogin")

# 3. BackgroundGameplayClassroom (Fit for classroom half 960x1080 and full 1920x1080)
# For the left 960x1080 half, we want the teacher desk and chalkboard centrally positioned
scale_gp = 1080 / SH
nw_gp, nh_gp = int(SW * scale_gp), 1080
scaled_gp = src_img.resize((nw_gp, nh_gp), Image.Resampling.LANCZOS)
# Crop center 960 width
left_gp = (nw_gp - 960) // 2
bg_gp_classroom = scaled_gp.crop((left_gp, 0, left_gp + 960, 1080))
save_both(bg_gp_classroom, "BackgroundGameplayClassroom")

# 4. BackgroundGameplayQuiz (960x1080 right half panel)
bg_gp_quiz = Image.new("RGBA", (960, 1080), (245, 248, 250, 255))
qdraw = ImageDraw.Draw(bg_gp_quiz)
# Soft subtle chalkboard-green / wood header and warm grid pattern
qdraw.rectangle([0, 0, 960, 100], fill=COLOR_BOARD_GREEN)
qdraw.rectangle([0, 95, 960, 105], fill=COLOR_WOOD_MID)
qdraw.rectangle([0, 105, 960, 108], fill=COLOR_GOLD)
# Subtle soft classroom lines
for ly in range(160, 1060, 40):
    qdraw.line([30, ly, 930, ly], fill=(225, 235, 240, 180), width=1)
# Add decorative bunting at top of quiz panel
flags = [COLOR_RED_FLAG, COLOR_YELLOW_FLAG, COLOR_BLUE_FLAG, COLOR_GREEN_FLAG]
for i, fx in enumerate(range(40, 920, 80)):
    fc = flags[i % len(flags)]
    qdraw.polygon([(fx, 0), (fx + 60, 0), (fx + 30, 45)], fill=fc)
save_both(bg_gp_quiz, "BackgroundGameplayQuiz")

# 5. BackgroundEndScene (1920x1080 celebratory classroom with confetti and golden stars)
bg_end = create_16_9_background(src_img, 1920, 1080, blur=1)
end_overlay = Image.new("RGBA", (1920, 1080), (0, 0, 0, 0))
edraw = ImageDraw.Draw(end_overlay)
random.seed(42)
# Draw golden stars and confetti
for _ in range(120):
    sx = random.randint(50, 1870)
    sy = random.randint(30, 750)
    ss = random.randint(8, 22)
    scolor = random.choice([COLOR_GOLD, COLOR_RED_FLAG, COLOR_YELLOW_FLAG, COLOR_BLUE_FLAG, (255, 255, 255, 240)])
    # 4-point star or confetti rect
    if random.random() > 0.4:
        # Star
        edraw.polygon([(sx, sy - ss), (sx + ss//3, sy - ss//3), (sx + ss, sy), (sx + ss//3, sy + ss//3),
                       (sx, sy + ss), (sx - ss//3, sy + ss//3), (sx - ss, sy), (sx - ss//3, sy - ss//3)], fill=scolor)
    else:
        # Confetti ribbon
        ang = random.randint(-45, 45)
        edraw.rounded_rectangle([sx, sy, sx + ss*1.5, sy + ss//2], radius=3, fill=scolor)
bg_end = Image.alpha_composite(bg_end, end_overlay)
save_both(bg_end, "BackgroundEndScene")

# -------------------------------------------------------------
# 9-SLICED UI SPRITES (Created at 2x resolution and downscaled for crisp anti-aliasing)
# -------------------------------------------------------------

def create_board_frame(size=512, border=42):
    """9-sliced Blackboard with rich wood frame, gold inlay, and dark chalkboard green"""
    scale = 2
    S = size * scale
    B = border * scale
    img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    
    # Outer wood frame with 3D bevel
    corner_radius = 28 * scale
    # Outer dark wood shadow
    d.rounded_rectangle([4*scale, 4*scale, S - 4*scale, S - 4*scale], radius=corner_radius, fill=COLOR_WOOD_DARK)
    # Main warm wood body
    d.rounded_rectangle([8*scale, 8*scale, S - 8*scale, S - 12*scale], radius=corner_radius - 2*scale, fill=COLOR_WOOD_MID)
    # Top wood highlight
    d.rounded_rectangle([10*scale, 10*scale, S - 10*scale, S - 20*scale], radius=corner_radius - 4*scale, fill=COLOR_WOOD_LIGHT)
    # Inner wood shadow
    d.rounded_rectangle([20*scale, 20*scale, S - 20*scale, S - 20*scale], radius=corner_radius - 8*scale, fill=COLOR_WOOD_DARK)
    
    # Golden inlay rim
    d.rounded_rectangle([26*scale, 26*scale, S - 26*scale, S - 26*scale], radius=corner_radius - 12*scale, fill=COLOR_GOLD)
    d.rounded_rectangle([30*scale, 30*scale, S - 30*scale, S - 30*scale], radius=corner_radius - 14*scale, fill=COLOR_GOLD_DARK)
    
    # Blackboard center
    d.rounded_rectangle([34*scale, 34*scale, S - 34*scale, S - 34*scale], radius=corner_radius - 16*scale, fill=COLOR_BOARD_GREEN)
    
    # Subtle inner bevel shadow on blackboard
    d.rounded_rectangle([34*scale, 34*scale, S - 34*scale, 46*scale], radius=6*scale, fill=(15, 45, 30, 160))
    
    res = img.resize((size, size), Image.Resampling.LANCZOS)
    return res

board_frame = create_board_frame(512, 42)
save_both(board_frame, "BoardFrame")

def create_paper_card(size=512, border=32):
    """9-sliced School Paper / Question Card with clean chalk-white body and warm wood/gold border"""
    scale = 2
    S = size * scale
    img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    cr = 26 * scale
    
    # Drop shadow
    d.rounded_rectangle([8*scale, 14*scale, S - 8*scale, S - 2*scale], radius=cr, fill=(0, 0, 0, 35))
    # Outer wood/warm border
    d.rounded_rectangle([8*scale, 8*scale, S - 8*scale, S - 8*scale], radius=cr, fill=COLOR_WOOD_MID)
    # Gold rim
    d.rounded_rectangle([12*scale, 12*scale, S - 12*scale, S - 12*scale], radius=cr - 4*scale, fill=COLOR_GOLD)
    # Clean warm paper body
    d.rounded_rectangle([16*scale, 16*scale, S - 16*scale, S - 16*scale], radius=cr - 8*scale, fill=(255, 253, 248, 255))
    
    # Subtle header bar zone
    d.rounded_rectangle([16*scale, 16*scale, S - 16*scale, 56*scale], radius=cr - 8*scale, fill=(245, 240, 230, 255))
    
    res = img.resize((size, size), Image.Resampling.LANCZOS)
    return res

paper_card = create_paper_card(512, 32)
save_both(paper_card, "QuestionCard")
save_both(paper_card, "SchoolPaperCard")

def create_button(width=512, height=256, border=28, main_color=COLOR_GOLD, shadow_color=COLOR_WOOD_DARK, highlight_color=(254, 235, 150, 255)):
    """9-sliced 3D Cartoon Button with rounded corners, top highlight, and bottom bevel"""
    scale = 2
    W = width * scale
    H = height * scale
    cr = 34 * scale
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    
    # Deep bottom shadow
    d.rounded_rectangle([8*scale, 16*scale, W - 8*scale, H - 4*scale], radius=cr, fill=shadow_color)
    # Main button face (shifted up by 10px to create 3D pressable ledge)
    d.rounded_rectangle([8*scale, 6*scale, W - 8*scale, H - 20*scale], radius=cr, fill=main_color)
    # Top highlight curve
    d.rounded_rectangle([16*scale, 12*scale, W - 16*scale, H // 2], radius=cr - 6*scale, fill=highlight_color)
    # Clean dark outline
    d.rounded_rectangle([8*scale, 6*scale, W - 8*scale, H - 20*scale], radius=cr, outline=(50, 25, 10, 180), width=4*scale)
    
    res = img.resize((width, height), Image.Resampling.LANCZOS)
    return res

# 1. Primary Classroom Gold Button (Play, Enter, Confirm)
btn_gold = create_button(512, 256, 28, COLOR_GOLD, (130, 75, 15, 255), (255, 244, 180, 200))
save_both(btn_gold, "ClassroomButton")

# 2. Chalkboard Green Button (Quiz Answers, Success)
btn_chalk = create_button(512, 256, 28, (39, 174, 96, 255), (20, 90, 50, 255), (130, 224, 170, 180))
save_both(btn_chalk, "ChalkButton")

# 3. School Blue Button (Register, Info, Secondary)
btn_blue = create_button(512, 256, 28, (41, 128, 185, 255), (21, 67, 96, 255), (133, 193, 233, 180))
save_both(btn_blue, "ClassroomButtonBlue")

# 4. Danger Red Button (Exit, End Session)
btn_red = create_button(512, 256, 28, (231, 76, 60, 255), (120, 40, 31, 255), (245, 183, 177, 180))
save_both(btn_red, "ClassroomButtonDanger")

# 5. Podium Cards (Gold, Silver, Bronze)
def create_podium_card(color_border, color_fill, size=(512, 256)):
    scale = 2
    W, H = size[0] * scale, size[1] * scale
    cr = 26 * scale
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    # Shadow
    d.rounded_rectangle([6*scale, 12*scale, W - 6*scale, H - 2*scale], radius=cr, fill=(0, 0, 0, 30))
    # Outer border
    d.rounded_rectangle([6*scale, 6*scale, W - 6*scale, H - 6*scale], radius=cr, fill=color_border)
    # Fill
    d.rounded_rectangle([14*scale, 14*scale, W - 14*scale, H - 14*scale], radius=cr - 8*scale, fill=color_fill)
    return img.resize(size, Image.Resampling.LANCZOS)

card_gold = create_podium_card(COLOR_GOLD, (255, 251, 235, 255))
card_silver = create_podium_card((189, 195, 199, 255), (248, 249, 250, 255))
card_bronze = create_podium_card((186, 110, 56, 255), (253, 245, 238, 255))
save_both(card_gold, "PodiumCardGold")
save_both(card_silver, "PodiumCardSilver")
save_both(card_bronze, "PodiumCardBronze")

# -------------------------------------------------------------
# TROPHY & MEDAL SPRITES (Inspired directly by the poster in the image!)
# -------------------------------------------------------------

def draw_trophy(size=512):
    """Championship Golden Trophy cup matching the CLASSROOM TRIVIA poster in the image"""
    scale = 2
    S = size * scale
    img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    cx, cy = S // 2, S // 2
    
    # Pedestal Base
    base_w, base_h = 240 * scale, 70 * scale
    base_top = cy + 150 * scale
    # Base dark wood
    d.rounded_rectangle([cx - base_w//2, base_top, cx + base_w//2, base_top + base_h], radius=16*scale, fill=COLOR_WOOD_DARK)
    d.rounded_rectangle([cx - base_w//2 + 8*scale, base_top + 4*scale, cx + base_w//2 - 8*scale, base_top + base_h - 14*scale], radius=12*scale, fill=COLOR_WOOD_MID)
    # Gold nameplate
    d.rounded_rectangle([cx - 80*scale, base_top + 16*scale, cx + 80*scale, base_top + 48*scale], radius=6*scale, fill=COLOR_GOLD)
    
    # Stem
    stem_w = 50 * scale
    stem_top = cy + 90 * scale
    d.rounded_rectangle([cx - stem_w//2, stem_top, cx + stem_w//2, base_top], radius=8*scale, fill=COLOR_GOLD_DARK)
    d.rounded_rectangle([cx - stem_w//4, stem_top, cx + stem_w//4, base_top], radius=4*scale, fill=COLOR_GOLD)
    
    # Handles (Left and Right)
    handle_r = 75 * scale
    d.ellipse([cx - 180*scale, cy - 80*scale, cx - 70*scale, cy + 50*scale], fill=COLOR_GOLD_DARK)
    d.ellipse([cx - 160*scale, cy - 60*scale, cx - 90*scale, cy + 30*scale], fill=(0, 0, 0, 0))
    d.ellipse([cx + 70*scale, cy - 80*scale, cx + 180*scale, cy + 50*scale], fill=COLOR_GOLD_DARK)
    d.ellipse([cx + 90*scale, cy - 60*scale, cx + 160*scale, cy + 30*scale], fill=(0, 0, 0, 0))
    
    # Main Cup Body
    cup_top = cy - 130 * scale
    cup_bottom = cy + 100 * scale
    cup_w = 220 * scale
    # Shaded body
    d.polygon([(cx - cup_w//2, cup_top), (cx + cup_w//2, cup_top),
               (cx + cup_w//3, cup_bottom), (cx - cup_w//3, cup_bottom)], fill=COLOR_GOLD_DARK)
    # Rounded cup bottom
    d.chord([cx - cup_w//3, cup_bottom - 40*scale, cx + cup_w//3, cup_bottom + 40*scale], 0, 180, fill=COLOR_GOLD_DARK)
    
    # Main golden face
    d.polygon([(cx - cup_w//2 + 14*scale, cup_top + 8*scale), (cx + cup_w//2 - 14*scale, cup_top + 8*scale),
               (cx + cup_w//3 - 10*scale, cup_bottom - 10*scale), (cx - cup_w//3 + 10*scale, cup_bottom - 10*scale)], fill=COLOR_GOLD)
    
    # Cup rim
    d.rounded_rectangle([cx - cup_w//2 - 16*scale, cup_top - 16*scale, cx + cup_w//2 + 16*scale, cup_top + 16*scale], radius=16*scale, fill=COLOR_GOLD)
    d.rounded_rectangle([cx - cup_w//2 - 8*scale, cup_top - 10*scale, cx + cup_w//2 + 8*scale, cup_top + 6*scale], radius=10*scale, fill=(255, 245, 170, 255))
    
    # 5-pointed Star on the cup center
    star_cy = cy - 20 * scale
    star_r = 45 * scale
    star_pts = []
    for i in range(10):
        angle = i * math.pi / 5 - math.pi / 2
        r = star_r if i % 2 == 0 else star_r * 0.45
        star_pts.append((cx + r * math.cos(angle), star_cy + r * math.sin(angle)))
    d.polygon(star_pts, fill=(255, 255, 255, 240))
    
    # Sparkle glint
    glint_x, glint_y = cx + 70*scale, cup_top + 20*scale
    d.ellipse([glint_x - 12*scale, glint_y - 12*scale, glint_x + 12*scale, glint_y + 12*scale], fill=(255, 255, 255, 255))
    
    return img.resize((size, size), Image.Resampling.LANCZOS)

trophy = draw_trophy(512)
save_both(trophy, "TrophyGold")

def draw_medal(rank, main_color, rim_color, ribbon_color, size=256):
    """Polished round classroom medal with ribbon for podium"""
    scale = 2
    S = size * scale
    img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    cx, cy = S // 2, S // 2 + 25 * scale
    
    # Ribbon hanging behind medal
    rw = 55 * scale
    rh = 120 * scale
    d.polygon([(cx - rw, 10*scale), (cx - rw + 30*scale, 10*scale), (cx, cy), (cx - 30*scale, cy)], fill=ribbon_color)
    d.polygon([(cx + rw - 30*scale, 10*scale), (cx + rw, 10*scale), (cx + 30*scale, cy), (cx, cy)], fill=ribbon_color)
    d.polygon([(cx - 15*scale, 10*scale), (cx + 15*scale, 10*scale), (cx, cy)], fill=(255, 255, 255, 220))
    
    # Outer medal ring shadow
    mr = 85 * scale
    d.ellipse([cx - mr, cy - mr + 6*scale, cx + mr, cy + mr + 6*scale], fill=(0, 0, 0, 50))
    d.ellipse([cx - mr, cy - mr, cx + mr, cy + mr], fill=rim_color)
    # Inner face
    d.ellipse([cx - mr + 10*scale, cy - mr + 10*scale, cx + mr - 10*scale, cy + mr - 10*scale], fill=main_color)
    # Highlight arc
    d.arc([cx - mr + 18*scale, cy - mr + 18*scale, cx + mr - 18*scale, cy + mr - 18*scale], 190, 350, fill=(255, 255, 255, 200), width=6*scale)
    
    # Rank digit / Star
    num = str(rank)
    # Draw simple embossed number in center using polygons/rects
    num_scale = scale
    if rank == 1:
        # Digit 1
        d.rounded_rectangle([cx - 8*scale, cy - 35*scale, cx + 12*scale, cy + 35*scale], radius=4*scale, fill=(255, 255, 255, 255))
        d.polygon([(cx - 8*scale, cy - 15*scale), (cx - 28*scale, cy - 5*scale), (cx - 8*scale, cy - 35*scale)], fill=(255, 255, 255, 255))
        d.rounded_rectangle([cx - 25*scale, cy + 28*scale, cx + 29*scale, cy + 38*scale], radius=3*scale, fill=(255, 255, 255, 255))
    elif rank == 2:
        # Digit 2
        d.arc([cx - 25*scale, cy - 35*scale, cx + 25*scale, cy + 5*scale], 180, 360, fill=(255, 255, 255, 255), width=16*scale)
        d.line([cx + 18*scale, cy - 10*scale, cx - 22*scale, cy + 28*scale], fill=(255, 255, 255, 255), width=16*scale)
        d.rounded_rectangle([cx - 28*scale, cy + 24*scale, cx + 28*scale, cy + 38*scale], radius=4*scale, fill=(255, 255, 255, 255))
    else:
        # Digit 3
        d.arc([cx - 25*scale, cy - 35*scale, cx + 20*scale, cy - 2*scale], 230, 80, fill=(255, 255, 255, 255), width=14*scale)
        d.arc([cx - 25*scale, cy - 4*scale, cx + 20*scale, cy + 35*scale], 270, 130, fill=(255, 255, 255, 255), width=14*scale)
        
    return img.resize((size, size), Image.Resampling.LANCZOS)

medal_gold = draw_medal(1, COLOR_GOLD, (198, 149, 10, 255), COLOR_RED_FLAG, 256)
medal_silver = draw_medal(2, (209, 213, 219, 255), (156, 163, 175, 255), COLOR_BLUE_FLAG, 256)
medal_bronze = draw_medal(3, (202, 138, 4, 255), (146, 64, 14, 255), (230, 126, 34, 255), 256)

save_both(medal_gold, "MedalGold")
save_both(medal_silver, "MedalSilver")
save_both(medal_bronze, "MedalBronze")

# -------------------------------------------------------------
# DECORATIVE SPRITES (Bunting Banner, Potted Plant, Avatars)
# -------------------------------------------------------------

def create_bunting_banner(width=1920, height=160):
    """Full width hanging festive pennant flags"""
    img = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    # String curves
    colors = [COLOR_RED_FLAG, COLOR_YELLOW_FLAG, COLOR_BLUE_FLAG, COLOR_GREEN_FLAG, (155, 89, 182, 255)]
    span = 95
    for i, x in enumerate(range(10, width - 60, span)):
        c = colors[i % len(colors)]
        y_top = int(15 + 20 * math.sin(i * 0.4))
        flag_h = 70
        d.polygon([(x, y_top), (x + span - 10, y_top), (x + span // 2, y_top + flag_h)], fill=c)
        d.polygon([(x, y_top), (x + span - 10, y_top), (x + span // 2, y_top + flag_h)], outline=(0, 0, 0, 60), width=2)
    # String
    for x in range(0, width, 5):
        y = int(15 + 20 * math.sin((x / span) * 0.4))
        d.line([x, y, x + 5, y], fill=(80, 80, 80, 180), width=3)
    return img

bunting = create_bunting_banner(1920, 160)
save_both(bunting, "BuntingBanner")

def create_potted_plant(width=256, height=384):
    """Cute potted plant in terracotta pot matching the corners of the image"""
    scale = 2
    W, H = width * scale, height * scale
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    cx = W // 2
    
    # Leaves
    leaf_color = (46, 204, 113, 255)
    leaf_dark = (39, 174, 96, 255)
    # Center leaf
    d.ellipse([cx - 40*scale, 50*scale, cx + 40*scale, 200*scale], fill=leaf_color, outline=(20, 100, 50, 255), width=4*scale)
    # Left leaf
    d.ellipse([cx - 100*scale, 90*scale, cx - 10*scale, 220*scale], fill=leaf_dark, outline=(20, 100, 50, 255), width=4*scale)
    # Right leaf
    d.ellipse([cx + 10*scale, 90*scale, cx + 100*scale, 220*scale], fill=leaf_color, outline=(20, 100, 50, 255), width=4*scale)
    
    # Terracotta Pot
    pot_top = 210 * scale
    pot_bottom = H - 30 * scale
    pot_top_w = 90 * scale
    pot_bot_w = 65 * scale
    pot_color = (211, 84, 0, 255)
    pot_rim_color = (230, 126, 34, 255)
    
    # Pot body
    d.polygon([(cx - pot_top_w, pot_top), (cx + pot_top_w, pot_top),
               (cx + pot_bot_w, pot_bottom), (cx - pot_bot_w, pot_bottom)], fill=pot_color, outline=(120, 40, 0, 255), width=4*scale)
    # Pot rim
    d.rounded_rectangle([cx - pot_top_w - 10*scale, pot_top - 20*scale, cx + pot_top_w + 10*scale, pot_top + 15*scale],
                        radius=8*scale, fill=pot_rim_color, outline=(120, 40, 0, 255), width=4*scale)
    
    return img.resize((width, height), Image.Resampling.LANCZOS)

plant = create_potted_plant(256, 384)
save_both(plant, "PottedPlant")

def create_avatar(is_teacher=False, size=256):
    """Cute cartoon character avatar"""
    scale = 2
    S = size * scale
    img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    cx, cy = S // 2, S // 2
    
    # Outer badge circle
    border_col = COLOR_GOLD if is_teacher else COLOR_WOOD_MID
    fill_col = (255, 250, 240, 255)
    d.ellipse([10*scale, 10*scale, S - 10*scale, S - 10*scale], fill=border_col)
    d.ellipse([18*scale, 18*scale, S - 18*scale, S - 18*scale], fill=fill_col)
    
    # Character Head
    head_r = 50 * scale
    head_cy = cy - 10 * scale
    d.ellipse([cx - head_r, head_cy - head_r, cx + head_r, head_cy + head_r], fill=(255, 224, 189, 255), outline=(120, 70, 30, 255), width=4*scale)
    
    # Eyes & Smile
    d.ellipse([cx - 22*scale, head_cy - 8*scale, cx - 12*scale, head_cy + 8*scale], fill=(40, 40, 40, 255))
    d.ellipse([cx + 12*scale, head_cy - 8*scale, cx + 22*scale, head_cy + 8*scale], fill=(40, 40, 40, 255))
    d.arc([cx - 16*scale, head_cy + 6*scale, cx + 16*scale, head_cy + 26*scale], 20, 160, fill=(180, 50, 50, 255), width=4*scale)
    
    # Shoulders / Shirt
    shirt_col = COLOR_BOARD_GREEN if is_teacher else (52, 152, 219, 255)
    d.chord([cx - 80*scale, cy + 30*scale, cx + 80*scale, S - 20*scale], 180, 360, fill=shirt_col, outline=(30, 30, 30, 255), width=4*scale)
    
    # Teacher Glasses & Graduation cap or hair
    if is_teacher:
        # Glasses
        d.rectangle([cx - 30*scale, head_cy - 12*scale, cx - 6*scale, head_cy + 12*scale], outline=(80, 40, 10, 255), width=3*scale)
        d.rectangle([cx + 6*scale, head_cy - 12*scale, cx + 30*scale, head_cy + 12*scale], outline=(80, 40, 10, 255), width=3*scale)
        d.line([cx - 6*scale, head_cy, cx + 6*scale, head_cy], fill=(80, 40, 10, 255), width=3*scale)
        # Graduation Cap
        cap_y = head_cy - head_r - 5 * scale
        d.polygon([(cx, cap_y - 30*scale), (cx + 65*scale, cap_y), (cx, cap_y + 15*scale), (cx - 65*scale, cap_y)], fill=(30, 30, 30, 255))
        d.line([cx + 50*scale, cap_y, cx + 55*scale, cap_y + 40*scale], fill=COLOR_GOLD, width=3*scale)
    else:
        # Student Hair
        d.chord([cx - head_r, head_cy - head_r - 5*scale, cx + head_r, head_cy + 10*scale], 180, 360, fill=(80, 45, 20, 255))
        
    return img.resize((size, size), Image.Resampling.LANCZOS)

teacher_avatar = create_avatar(True, 256)
student_avatar = create_avatar(False, 256)
save_both(teacher_avatar, "TeacherAvatar")
save_both(student_avatar, "StudentAvatar")

print("All sprites and backgrounds generated successfully!")
