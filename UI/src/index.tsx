import { ModRegistrar } from "cs2/modding";
import { HelloWorldComponent } from "mods/hello-world";
import { TransformSection } from "./mods/TransformSection";

const register: ModRegistrar = (moduleRegistry) => {

    //moduleRegistry.extend("game-ui/game/components/selected-info-panel/selected-info-sections/selected-info-sections.tsx", 'selectedInfoSectionComponents', TransformSection);
    moduleRegistry.append('Menu', HelloWorldComponent);
}

export default register;