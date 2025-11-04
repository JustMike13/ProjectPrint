import bpy
import math

class OBJECT_OT_generate_printable(bpy.types.Operator):
    bl_idname = "object.generate_printable"
    bl_label = "Generate Printable Object"
    bl_description = "Automates slicing and boolean intersection for printable geometry"
    bl_options = {'REGISTER', 'UNDO'}

    def execute(self, context):
        model = context.active_object
        if not model:
            self.report({'ERROR'}, "No active object selected.")
            return {'CANCELLED'}

        # === STEP 1.1: Scale model by 100 and apply ===
        model.scale = (100, 100, 100)
        bpy.context.view_layer.objects.active = model
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

        # === STEP 2: Create slicing planes ===
        height = model.dimensions.z
        plane_objs = []
        for i in range(num_planes):
            z = i * (height / num_planes)
            bpy.ops.mesh.primitive_plane_add(size=plane_size, location=(0, 0, z))
            plane = bpy.context.active_object
            plane.name = f"Slice_{i:03d}"
            plane.scale.z = height / num_planes  # STEP 4: scale thickness
            plane_objs.append(plane)

        # === STEP 3: Join planes into one object ===
        bpy.ops.object.select_all(action='DESELECT')
        for obj in plane_objs:
            obj.select_set(True)
        bpy.context.view_layer.objects.active = plane_objs[0]
        bpy.ops.object.join()
        slicer = bpy.context.active_object
        slicer.name = "SlicingPlanes"

        # === STEP 5: Boolean Intersection ===
        bool_mod = slicer.modifiers.new(name="SliceIntersect", type='BOOLEAN')
        bool_mod.operation = 'INTERSECT'
        bool_mod.object = model
        bpy.context.view_layer.objects.active = slicer
        bpy.ops.object.modifier_apply(modifier=bool_mod.name)

        # === STEP 6: Join sliced geometry to model (optional) ===
        model.select_set(True)
        slicer.select_set(True)
        bpy.context.view_layer.objects.active = model
        bpy.ops.object.join()

        # === STEP 7: Rotate for Unity ===
        if apply_rotation_for_unity:
            model.rotation_euler[0] += math.radians(90)

        # print("✅ Slicing complete with model scaled by 100.")

        self.report({'INFO'}, "✅ Printable object generated.")
        return {'FINISHED'}

class VIEW3D_PT_printable_panel(bpy.types.Panel):
    bl_label = "Printable Object Tools"
    bl_idname = "VIEW3D_PT_printable_panel"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = 'Printable'

    def draw(self, context):
        layout = self.layout
        layout.operator("object.generate_printable")

def register():
    bpy.utils.register_class(OBJECT_OT_generate_printable)
    bpy.utils.register_class(VIEW3D_PT_printable_panel)

def unregister():
    bpy.utils.unregister_class(OBJECT_OT_generate_printable)
    bpy.utils.unregister_class(VIEW3D_PT_printable_panel)

if __name__ == "__main__":
    register()