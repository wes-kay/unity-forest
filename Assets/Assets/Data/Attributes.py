skills = [
    # Primary Attributes
    ("strn", "Strength"),
    ("endu", "Endurance"),
    ("cons", "Constitution"),
    ("agil", "Agility"),
    ("dext", "Dexterity"),
    ("refl", "Reflexes"),
    ("perc", "Perception"),
    ("memo", "Memory"),

    # Resources
    ("cvit", "Current Vitality"),
    ("mvit", "Maximum Vitality"),

    ("csta", "Current Stamina"),
    ("msta", "Maximum Stamina"),

    ("cwil", "Current Willpower"),
    ("mwil", "Maximum Willpower"),

    ("cfoc", "Current Focus"),
    ("mfoc", "Maximum Focus"),

    ("cbld", "Current Blood"),
    ("mbld", "Maximum Blood"),

    ("csan", "Current Sanity"),
    ("msan", "Maximum Sanity"),

    ("cmor", "Current Morale"),
    ("mmor", "Maximum Morale"),

    ("chng", "Current Hunger"),
    ("mhng", "Maximum Hunger"),

    ("cthr", "Current Thirst"),
    ("mthr", "Maximum Thirst"),

    ("cfat", "Current Fatigue"),
    ("mfat", "Maximum Fatigue"),

    ("cexp", "Current Exposure"),
    ("mexp", "Maximum Exposure"),

    ("cwrk", "Current Warmth"),
    ("mwrk", "Maximum Warmth"),

    ("cbur", "Current Burden"),
    ("mbur", "Maximum Burden"),

    # Body Parts
    ("ched", "Current Head Health"),
    ("mhed", "Maximum Head Health"),

    ("ctor", "Current Torso Health"),
    ("mtor", "Maximum Torso Health"),

    ("clar", "Current Left Arm Health"),
    ("mlar", "Maximum Left Arm Health"),

    ("crar", "Current Right Arm Health"),
    ("mrar", "Maximum Right Arm Health"),

    ("cllg", "Current Left Leg Health"),
    ("mllg", "Maximum Left Leg Health"),

    ("crlg", "Current Right Leg Health"),
    ("mrlg", "Maximum Right Leg Health"),

    # Negative Conditions
    ("cpai", "Current Pain"),
    ("mpai", "Maximum Pain"),

    ("cinf", "Current Infection"),
    ("minf", "Maximum Infection"),

    ("ccor", "Current Corruption"),
    ("mcor", "Maximum Corruption"),

    ("cfvr", "Current Fever"),
    ("mfvr", "Maximum Fever"),

    ("cstr", "Current Stress"),
    ("mstr", "Maximum Stress"),

    # Afflictions
    ("cble", "Current Bleeding"),
    ("mble", "Maximum Bleeding"),

    ("cven", "Current Venom"),
    ("mven", "Maximum Venom"),

    ("ccur", "Current Curse"),
    ("mcur", "Maximum Curse"),

    ("cmad", "Current Madness"),
    ("mmad", "Maximum Madness"),

    ("cmut", "Current Mutation"),
    ("mmut", "Maximum Mutation"),

    ("cpos", "Current Possession"),
    ("mpos", "Maximum Possession"),

    # Resistances
    ("pres", "Pain Resistance"),
    ("ires", "Infection Resistance"),
    ("corr", "Corruption Resistance"),
    ("sres", "Stress Resistance"),
    ("fres", "Fear Resistance"),
    ("mres", "Madness Resistance"),

    ("bres", "Bleeding Resistance"),
    ("vres", "Venom Resistance"),
    ("dres", "Disease Resistance"),

    ("cold", "Cold Resistance"),
    ("heat", "Heat Resistance"),

    ("firs", "Fire Resistance"),
    ("ices", "Frost Resistance"),
    ("shrs", "Shadow Resistance"),
    ("sors", "Soul Resistance"),
    ("cars", "Chaos Resistance"),
    ("hers", "Hex Resistance"),

    ("kres", "Knockdown Resistance"),
    ("stun", "Stun Resistance"),
    ("tors", "Torture Resistance"),
    ("poss", "Possession Resistance"),

    # Reputation / Alignment
    ("cfai", "Current Faith"),
    ("mfai", "Maximum Faith"),

    ("cher", "Current Heresy"),
    ("mher", "Maximum Heresy"),

    ("cdar", "Current Dark Insight"),
    ("mdar", "Maximum Dark Insight"),

    # Derived Combat Values
    ("armr", "Armor"),
    ("evas", "Evasion"),
    ("bloc", "Block Chance"),
    ("crit", "Critical Chance"),
    ("crdm", "Critical Damage"),
    ("accu", "Accuracy"),

    ("move", "Movement Speed"),
    ("init", "Initiative"),

    ("heal", "Healing Rate"),
    ("regn", "Vitality Regeneration"),

    ("carr", "Carry Capacity"),

    # Combat Skills
    ("swrd", "Swordsmanship"),
    ("axem", "Axe Mastery"),
    ("spea", "Spear Mastery"),
    ("mace", "Mace Mastery"),
    ("dagg", "Dagger Mastery"),
    ("arch", "Archery"),
    ("xbow", "Crossbows"),
    ("thrw", "Throwing"),
    ("unar", "Unarmed Combat"),
    ("parr", "Parrying"),
    ("dodg", "Dodging"),
    ("tact", "Tactics"),
    ("shld", "Shield Use"),
    ("dual", "Dual Wielding"),
    ("exec", "Execution"),
    ("tort", "Torture"),
    ("inti", "Intimidation"),

    # Survival
    ("fish", "Fishing"),
    ("hunt", "Hunting"),
    ("trap", "Trapping"),
    ("skin", "Skinning"),
    ("butc", "Butchery"),
    ("trac", "Tracking"),
    ("fora", "Foraging"),
    ("camp", "Camping"),
    ("navi", "Navigation"),
    ("swim", "Swimming"),
    ("clim", "Climbing"),
    ("surv", "Survival"),
    ("scav", "Scavenging"),
    ("corp", "Corpse Harvesting"),

    # Gathering
    ("mine", "Mining"),
    ("lumb", "Lumberjacking"),
    ("harv", "Harvesting"),
    ("herb", "Herbalism"),
    ("bldh", "Blood Harvesting"),
    ("bone", "Bone Gathering"),
    ("reli", "Relic Hunting"),

    # Crafting
    ("blks", "Blacksmithing"),
    ("wpsm", "Weaponsmithing"),
    ("arsm", "Armorsmithing"),
    ("tail", "Tailoring"),
    ("leat", "Leatherworking"),
    ("bons", "Bonesmithing"),
    ("fles", "Fleshcrafting"),
    ("alch", "Alchemy"),
    ("pois", "Poisoncraft"),
    ("trmk", "Trap Making"),
    ("rune", "Runecrafting"),
    ("ench", "Enchanting"),
    ("engi", "Engineering"),

    # Medical
    ("faid", "First Aid"),
    ("surg", "Surgery"),
    ("anat", "Anatomy"),
    ("diag", "Diagnosis"),
    ("toxi", "Toxicology"),
    ("emba", "Embalming"),
    ("auto", "Autopsy"),
    ("ampu", "Amputation"),

    # Knowledge
    ("read", "Reading"),
    ("writ", "Writing"),
    ("rese", "Research"),
    ("inve", "Investigation"),
    ("hist", "History"),
    ("laws", "Law"),
    ("theo", "Theology"),
    ("occu", "Occultism"),
    ("demo", "Demonology"),
    ("necr", "Necrology"),
    ("astr", "Astrology"),
    ("ritu", "Ritual Lore"),
    ("mons", "Monster Lore"),

    # Social
    ("pers", "Persuasion"),
    ("nego", "Negotiation"),
    ("lead", "Leadership"),
    ("dece", "Deception"),
    ("etiq", "Etiquette"),
    ("inte", "Interrogation"),
    ("comm", "Command"),
    ("fana", "Fanaticism"),

    # Criminal
    ("stea", "Stealth"),
    ("pick", "Pickpocketing"),
    ("lock", "Lockpicking"),
    ("forg", "Forgery"),
    ("espi", "Espionage"),
    ("saba", "Sabotage"),
    ("assa", "Assassination"),
    ("smug", "Smuggling"),

    # Trade
    ("trad", "Trading"),
    ("appr", "Appraisal"),
    ("bart", "Bartering"),
    ("logi", "Logistics"),

    # Faith Skills
    ("fait", "Faith"),
    ("pray", "Prayer"),
    ("medi", "Meditation"),
    ("exor", "Exorcism"),
    ("bles", "Blessing"),
    ("curs", "Cursing"),
    ("sacr", "Sacrifice"),

    # Magic
    ("arca", "Arcane Knowledge"),
    ("fire", "Fire Magic"),
    ("icem", "Ice Magic"),
    ("blod", "Blood Magic"),
    ("necm", "Necromancy"),
    ("summ", "Summoning"),
    ("illu", "Illusion"),
    ("divi", "Divination"),
    ("shad", "Shadow Magic"),
    ("spir", "Spirit Binding"),
    ("hexc", "Hexcraft"),
    ("plag", "Plaguecraft"),
    ("soul", "Soul Manipulation"),

    # Eldritch / Corruption
    ("dark", "Dark Insight"),
    ("drea", "Dreamwalking"),
    ("void", "Void Affinity"),
    ("chao", "Chaos Attunement"),

    # Grimdark Utility
    ("grav", "Grave Digging"),
    ("mort", "Mortuary Work"),
    ("buri", "Burial Rites"),
    ("pilg", "Pilgrimage"),
    ("vigi", "Vigil Keeping"),
    ("here", "Heresy Detection"),

    ("cann", "Cannibalism"),
    ("blst", "Bloodletting"),
    ("muta", "Mutation Control"),
    ("rean", "Reanimation"),
    ("obse", "Obsession"),
]