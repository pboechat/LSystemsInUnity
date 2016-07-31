using UnityEngine;
using System;

public class ProceduralMeshes
{

    private ProceduralMeshes()
    {
    }

    public static Mesh CreateXYPlane(float width, float height, int xSegments, int ySegments, Vector3 center)
    {
        Mesh plane = new Mesh();

        float xIncrement = width / (float)xSegments;
        float yIncrement = height / (float)ySegments;

        int vertexQuantity = (xSegments + 1) * (ySegments + 1);
        Vector3[] vertexBuffer = new Vector3[vertexQuantity];
        Vector3[] normalBuffer = new Vector3[vertexQuantity];
        Vector2[] textureCoordinateBuffer = new Vector2[vertexQuantity];
        int i = 0;
        Vector3 vScan = center + new Vector3(-(width * 0.5f), -(height * 0.5f), 0);
        for (int y = 0; y <= ySegments; y++)
        {
            Vector3 hScan = vScan;
            for (int x = 0; x <= xSegments; x++)
            {
                hScan += new Vector3(xIncrement, 0, 0);
                vertexBuffer[i] = new Vector3(hScan.x, hScan.y, hScan.z);
                normalBuffer[i] = Vector3.up;
                textureCoordinateBuffer[i] = new Vector2(x / (float)xSegments, y / (float)ySegments);
                i++;
            }
            vScan += new Vector3(0, yIncrement, 0);
        }

        i = 0;
        int[] indexBuffer = new int[(xSegments * ySegments * 2) * 3];
        for (int y = 1; y <= ySegments; y++)
        {
            for (int x = 0; x < xSegments; x++)
            {
                int i0 = (y * (xSegments + 1)) + x;
                int i1 = ((y - 1) * (xSegments + 1)) + x;
                int i2 = i1 + 1;
                int i3 = i0 + 1;

                indexBuffer[i++] = i2;
                indexBuffer[i++] = i1;
                indexBuffer[i++] = i0;

                indexBuffer[i++] = i2;
                indexBuffer[i++] = i0;
                indexBuffer[i++] = i3;
            }
        }

        plane.vertices = vertexBuffer;
        plane.triangles = indexBuffer;
        plane.normals = normalBuffer;
        plane.uv = textureCoordinateBuffer;

        return plane;
    }

    public static Mesh CreateXZPlane(float width, float depth, int xSegments, int zSegments, Vector3 center)
    {
        Mesh plane = new Mesh();

        float xIncrement = width / (float)xSegments;
        float zIncrement = depth / (float)zSegments;

        int vertexQuantity = (xSegments + 1) * (zSegments + 1);
        Vector3[] vertexBuffer = new Vector3[vertexQuantity];
        Vector3[] normalBuffer = new Vector3[vertexQuantity];
        Vector2[] textureCoordinateBuffer = new Vector2[vertexQuantity];
        int i = 0;
        Vector3 vScan = center + new Vector3(-(width * 0.5f), 0, -(depth * 0.5f));
        for (int z = 0; z <= zSegments; z++)
        {
            Vector3 hScan = vScan;
            for (int x = 0; x <= xSegments; x++)
            {
                hScan += new Vector3(xIncrement, 0, 0);
                vertexBuffer[i] = new Vector3(hScan.x, hScan.y, hScan.z);
                normalBuffer[i] = Vector3.up;
                textureCoordinateBuffer[i] = new Vector2(x / (float)xSegments, z / (float)zSegments);
                i++;
            }
            vScan += new Vector3(0, 0, zIncrement);
        }

        i = 0;
        int[] indexBuffer = new int[(xSegments * zSegments * 2) * 3];
        for (int z = 1; z <= zSegments; z++)
        {
            for (int x = 0; x < xSegments; x++)
            {
                int i0 = (z * (xSegments + 1)) + x;
                int i1 = ((z - 1) * (xSegments + 1)) + x;
                int i2 = i1 + 1;
                int i3 = i0 + 1;

                indexBuffer[i++] = i2;
                indexBuffer[i++] = i1;
                indexBuffer[i++] = i0;

                indexBuffer[i++] = i2;
                indexBuffer[i++] = i0;
                indexBuffer[i++] = i3;
            }
        }

        plane.vertices = vertexBuffer;
        plane.triangles = indexBuffer;
        plane.normals = normalBuffer;
        plane.uv = textureCoordinateBuffer;

        return plane;
    }

    public static Mesh CreateCylinder(int axisSamples, int radialSamples, float radius, float height)
    {
        Mesh mesh = new Mesh();

        int numVertices = axisSamples * (radialSamples + 1);
        int numIndices = 3 * (2 * (axisSamples - 1) * radialSamples);

        // Create a vertex buffer.
        Vector3[] vertices = new Vector3[numVertices];
        Vector3[] normals = new Vector3[numVertices];
        Vector2[] uvs = new Vector2[numVertices];
        int[] indices = new int[numIndices];

        // Generate geometry.
        float invRS = 1.0f / (float)radialSamples;
        float invASm1 = 1.0f / (float)(axisSamples - 1);
        int r, a, aStart, i;

        // Generate points on the unit circle to be used in computing the mesh points on a cylinder slice.
        float[] cs = new float[radialSamples + 1];
        float[] sn = new float[radialSamples + 1];
        for (r = 0; r < radialSamples; ++r)
        {
            float angle = Mathf.PI * 2 * invRS * r;
            cs[r] = Mathf.Cos(angle);
            sn[r] = Mathf.Sin(angle);
        }
        cs[radialSamples] = cs[0];
        sn[radialSamples] = sn[0];

        Vector2 uv;

        // Generate the cylinder itself.
        for (a = 0, i = 0; a < axisSamples; ++a)
        {
            float axisFraction = a * invASm1;  // in [0,1]
            float y = height * axisFraction;

            // Compute center of slice.
            Vector3 sliceCenter = new Vector3(0, y, 0);

            // Compute slice vertices with duplication at endpoint.
            int save = i;
            for (r = 0; r < radialSamples; ++r)
            {
                float radialFraction = r * invRS;  // in [0,1)
                Vector3 normal = new Vector3(sn[r], 0, cs[r]);

                vertices[i] = sliceCenter + radius * normal;
                normals[i] = normal.normalized;

                uv = new Vector2(radialFraction, axisFraction);
                uvs[i] = uv;

                ++i;
            }

            vertices[i] = vertices[save];
            normals[i] = normals[save];

            uv = new Vector2(1.0f, axisFraction);
            uvs[i] = uv;

            ++i;
        }

        // Generate indices.
        int c = 0;
        for (a = 0, aStart = 0; a < axisSamples - 1; ++a)
        {
            int i0 = aStart;
            int i1 = i0 + 1;
            aStart += radialSamples + 1;
            int i2 = aStart;
            int i3 = i2 + 1;
            for (i = 0; i < radialSamples; ++i)
            {
                indices[c++] = i0++;
                indices[c++] = i1;
                indices[c++] = i2;
                indices[c++] = i1++;
                indices[c++] = i3++;
                indices[c++] = i2++;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.RecalculateBounds();

        return mesh;
    }

    public static Mesh CreateSphere(float radius, int zSegments, int radialSegments)
    {
        Mesh sphere = new Mesh();

        int vertexQuantity = (zSegments - 2) * (radialSegments + 1) + 2;
        int triangleQuantity = 2 * (zSegments - 2) * radialSegments;

        Vector3[] vertexBuffer = new Vector3[vertexQuantity];
        Vector2[] textureCoordinateBuffer = new Vector2[vertexQuantity];
        Color[] colorBuffer = new Color[vertexQuantity];
        int[] indexBuffer = new int[3 * triangleQuantity];

        // generate geometry
        float radialFactor = 1.0F / (radialSegments);
        float zFactor = 2.0F / (zSegments - 1);

        // Generate points on the unit circle to be used in computing the mesh
        // points on a cylinder slice.
        float[] sines = new float[radialSegments + 1];
        float[] cossines = new float[radialSegments + 1];
        for (int radialIndex = 0; radialIndex < radialSegments; radialIndex++)
        {
            float angle = (2 * Mathf.PI) * radialFactor * radialIndex;
            cossines[radialIndex] = Mathf.Cos(angle);
            sines[radialIndex] = Mathf.Sin(angle);
        }

        sines[radialSegments] = sines[0];
        cossines[radialSegments] = cossines[0];

        // generate the sphere itself
        int i = 0;
        Vector3 vertex;
        for (int zIndex = 1; zIndex < zSegments - 1; zIndex++)
        {
            float zFraction = -1.0F + zFactor * zIndex;  // in (-1,1)
            float z = radius * zFraction;

            // compute center of slice
            Vector3 sliceCenter = new Vector3(0.0F, 0.0F, z);

            // compute radius of slice
            float sliceRadius = Mathf.Sqrt(Mathf.Abs(radius * radius - z * z));

            // compute slice vertices with duplication at end point
            int savedI = i;
            for (int radialIndex = 0; radialIndex < radialSegments; radialIndex++)
            {
                float radialFraction = radialIndex * radialFactor;  // in [0,1)
                Vector3 radial = new Vector3(cossines[radialIndex], sines[radialIndex], 0.0F);
                vertexBuffer[i] = sliceCenter + sliceRadius * radial;

                vertex = vertexBuffer[i];
                vertex.Normalize();
                colorBuffer[i] = new Color(vertex.x, vertex.y, vertex.z);

                textureCoordinateBuffer[i] = new Vector2(radialFraction, 0.5F * (zFraction + 1.0F));

                i++;
            }

            vertexBuffer[i] = vertexBuffer[savedI];

            vertex = vertexBuffer[i];
            vertex.Normalize();
            colorBuffer[i] = new Color(vertex.x, vertex.y, vertex.z);

            textureCoordinateBuffer[i] = new Vector2(1.0F, 0.5F * (zFraction + 1.0F));

            i++;
        }

        // south pole
        vertexBuffer[i] = -radius * Vector3.forward;

        vertex = vertexBuffer[i];
        vertex.Normalize();
        colorBuffer[i] = new Color(vertex.x, vertex.y, vertex.z);

        textureCoordinateBuffer[i] = new Vector2(0.5F, 0.5F);

        i++;

        // north pole
        vertexBuffer[i] = radius * Vector3.forward;

        vertex = vertexBuffer[i];
        vertex.Normalize();
        colorBuffer[i] = new Color(vertex.x, vertex.y, vertex.z);

        textureCoordinateBuffer[i] = new Vector2(0.5F, 1.0F);

        i++;

        if (i != vertexQuantity)
        {
            // TODO:
            throw new Exception("");
        }

        if (!(radialSegments < (32768 - 1) && (radialSegments >= 0)))
        {
            // TODO:
            throw new Exception("");
        }

        int radialSegmentsCount = radialSegments;
        if (!(vertexQuantity < (32768) && (vertexQuantity >= 0)))
        {
            // TODO:
            throw new Exception("");
        }

        int vq = vertexQuantity;
        if (!(zSegments < (32768) && (zSegments >= 0)))
        {
            // TODO:
            throw new Exception("");
        }

        int zSegmentsCount = zSegments;

        // generate connectivity
        int iZStart = 0;
        int connectivityIndex = 0;
        for (int iZ = 0; iZ < zSegments - 3; iZ++)
        {
            int i0 = iZStart;
            int i1 = i0 + 1;
            iZStart += radialSegmentsCount + 1;
            int i2 = iZStart;
            int i3 = i2 + 1;
            for (int j = 0; j < radialSegments; j++)
            {
                indexBuffer[connectivityIndex] = i0++;
                indexBuffer[connectivityIndex + 1] = i2;
                indexBuffer[connectivityIndex + 2] = i1;
                indexBuffer[connectivityIndex + 3] = i1++;
                indexBuffer[connectivityIndex + 4] = i2++;
                indexBuffer[connectivityIndex + 5] = i3++;
                connectivityIndex += 6;
            }
        }

        // south pole triangles
        int vQm2 = vq - 2;
        //c = 0;
        for (int j = 0; j < radialSegmentsCount; j++)
        {
            indexBuffer[connectivityIndex] = j;
            indexBuffer[connectivityIndex + 1] = j + 1;
            indexBuffer[connectivityIndex + 2] = vQm2;
            connectivityIndex += 3;
        }

        // north pole triangles
        int vQm1 = vq - 1;
        int offset = (zSegmentsCount - 3) * (radialSegmentsCount + 1);
        //c = 0;
        for (int j = 0; j < radialSegmentsCount; j++)
        {
            indexBuffer[connectivityIndex] = j + offset;
            indexBuffer[connectivityIndex + 1] = vQm1;
            indexBuffer[connectivityIndex + 2] = j + 1 + offset;
            connectivityIndex += 3;
        }

        if (indexBuffer.Length != 3 * triangleQuantity)
        {
            // TODO:
            throw new Exception("");
        }

        sphere.vertices = vertexBuffer;
        sphere.colors = colorBuffer;
        sphere.uv = textureCoordinateBuffer;

        // invert triangles to right-hand
        for (int j = 0; j < indexBuffer.Length; j += 3)
        {
            int a = indexBuffer[j];
            int c = indexBuffer[j + 2];
            indexBuffer[j] = c;
            indexBuffer[j + 2] = a;
        }

        sphere.triangles = indexBuffer;

        sphere.RecalculateNormals();
        // sphere.Optimize();

        return sphere;
    }
}
